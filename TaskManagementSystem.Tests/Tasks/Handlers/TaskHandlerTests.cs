using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Tasks;
using Xunit;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Features.Tasks.Create;
using TaskManagementSystem.Features.Tasks.Delete;
using TaskManagementSystem.Features.Tasks.GetById;
using TaskManagementSystem.Features.Tasks.List;
using TaskManagementSystem.Features.Tasks.SetCompletion;
using TaskManagementSystem.Features.Tasks.Update;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class TaskHandlerTests : IDisposable
{
    private readonly AppDbContext _db;

    public TaskHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    private async Task<TaskItem> SeedTask(string title = "Test Task", Priority priority = Priority.Medium, bool isCompleted = false)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = "Test description",
            IsCompleted = isCompleted,
            Priority = priority,
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    // ─── CreateTask ──────────────────────────────────────────────

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsSuccess()
    {
        var handler = new CreateTaskHandler(_db);
        var command = new CreateTaskCommand("New Task", "A description", Priority.High, DateTime.UtcNow.AddDays(5));

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New Task");
        result.Value.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task CreateTask_WithNullPriority_DefaultsToMedium()
    {
        var handler = new CreateTaskHandler(_db);
        var command = new CreateTaskCommand("Another Task", null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public async Task CreateTask_PersistsToDatabase()
    {
        var handler = new CreateTaskHandler(_db);
        var command = new CreateTaskCommand("Persisted Task", "desc", Priority.Low, null);

        var result = await handler.Handle(command, CancellationToken.None);

        var saved = await _db.Tasks.FindAsync(result.Value!.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Persisted Task");
    }

    // ─── CreateTask Validator ────────────────────────────────────

    [Fact]
    public async Task CreateTaskValidator_EmptyTitle_Fails()
    {
        var validator = new CreateTaskValidator();
        var command = new CreateTaskCommand("", null, null, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task CreateTaskValidator_TitleTooShort_Fails()
    {
        var validator = new CreateTaskValidator();
        var command = new CreateTaskCommand("Ab", null, null, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTaskValidator_InvalidPriority_Fails()
    {
        var validator = new CreateTaskValidator();
        var command = new CreateTaskCommand("Valid Title", null, (Priority)999, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Fact]
    public async Task CreateTaskValidator_ValidCommand_Passes()
    {
        var validator = new CreateTaskValidator();
        var command = new CreateTaskCommand("Valid Title", "Description", Priority.High, DateTime.UtcNow);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    // ─── GetTaskById ─────────────────────────────────────────────

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsSuccess()
    {
        var seeded = await SeedTask();
        var handler = new GetTaskByIdHandler(_db);

        var result = await handler.Handle(new GetTaskByIdQuery(seeded.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(seeded.Id);
        result.Value.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTaskById_NonExistingTask_ReturnsNotFound()
    {
        var handler = new GetTaskByIdHandler(_db);

        var result = await handler.Handle(new GetTaskByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    // ─── ListTasks ───────────────────────────────────────────────

    [Fact]
    public async Task ListTasks_NoFilter_ReturnsAll()
    {
        await SeedTask("Task 1");
        await SeedTask("Task 2", isCompleted: true);
        var handler = new ListTasksHandler(_db);

        var result = await handler.Handle(new ListTasksQuery(null), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListTasks_ActiveFilter_ReturnsOnlyActive()
    {
        await SeedTask("Active Task");
        await SeedTask("Completed Task", isCompleted: true);
        var handler = new ListTasksHandler(_db);

        var result = await handler.Handle(new ListTasksQuery(TaskStatusFilter.Active), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Active Task");
    }

    [Fact]
    public async Task ListTasks_CompletedFilter_ReturnsOnlyCompleted()
    {
        await SeedTask("Active Task");
        await SeedTask("Completed Task", isCompleted: true);
        var handler = new ListTasksHandler(_db);

        var result = await handler.Handle(new ListTasksQuery(TaskStatusFilter.Completed), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Completed Task");
    }

    // ─── UpdateTask ──────────────────────────────────────────────

    [Fact]
    public async Task UpdateTask_ExistingTask_ReturnsUpdated()
    {
        var seeded = await SeedTask();
        var handler = new UpdateTaskHandler(_db);
        var command = new UpdateTaskCommand(seeded.Id, "Updated Title", "Updated Desc", Priority.High, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated Title");
        result.Value.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task UpdateTask_NonExistingTask_ReturnsNotFound()
    {
        var handler = new UpdateTaskHandler(_db);
        var command = new UpdateTaskCommand(Guid.NewGuid(), "Title", null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task UpdateTask_NullPriority_KeepsExistingPriority()
    {
        var seeded = await SeedTask(priority: Priority.High);
        var handler = new UpdateTaskHandler(_db);
        var command = new UpdateTaskCommand(seeded.Id, "Updated", null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Priority.Should().Be(Priority.High);
    }

    // ─── SetTaskCompletion ───────────────────────────────────────

    [Fact]
    public async Task SetTaskCompletion_MarkComplete_SetsIsCompletedTrue()
    {
        var seeded = await SeedTask();
        var handler = new SetTaskCompletionHandler(_db);

        var result = await handler.Handle(new SetTaskCompletionCommand(seeded.Id, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task SetTaskCompletion_MarkIncomplete_SetsIsCompletedFalse()
    {
        var seeded = await SeedTask(isCompleted: true);
        var handler = new SetTaskCompletionHandler(_db);

        var result = await handler.Handle(new SetTaskCompletionCommand(seeded.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task SetTaskCompletion_NonExistingTask_ReturnsNotFound()
    {
        var handler = new SetTaskCompletionHandler(_db);

        var result = await handler.Handle(new SetTaskCompletionCommand(Guid.NewGuid(), true), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    // ─── DeleteTask ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteTask_ExistingTask_ReturnsSuccess()
    {
        var seeded = await SeedTask();
        var handler = new DeleteTaskHandler(_db);

        var result = await handler.Handle(new DeleteTaskCommand(seeded.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteTask_ExistingTask_RemovesFromDatabase()
    {
        var seeded = await SeedTask();
        var handler = new DeleteTaskHandler(_db);

        await handler.Handle(new DeleteTaskCommand(seeded.Id), CancellationToken.None);

        var deleted = await _db.Tasks.FindAsync(seeded.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTask_NonExistingTask_ReturnsNotFound()
    {
        var handler = new DeleteTaskHandler(_db);

        var result = await handler.Handle(new DeleteTaskCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }
}
