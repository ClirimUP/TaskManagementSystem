using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Update;
using TaskManagementSystem.Tests.Helpers;
using TaskManagementSystem.Tests.Tasks.Builders;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class UpdateTaskHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ExistingTask_ReturnsUpdatedValues()
    {
        // Arrange
        var task = TaskItemHelper.Generate(title: "Original Title", priority: Priority.Low, userId: _userId);

        using var builder = new UpdateTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var command = new UpdateTaskCommand(task.Id, "Updated Title", "Updated Desc", Priority.High, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated Title");
        result.Value.Description.Should().Be("Updated Desc");
        result.Value.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task Handle_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        using var builder = new UpdateTaskHandlerBuilder();
        var handler = builder.Build();
        var command = new UpdateTaskCommand(Guid.NewGuid(), "Title", null, null, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task Handle_NullPriority_KeepsExistingPriority()
    {
        // Arrange
        var task = TaskItemHelper.Generate(priority: Priority.High, userId: _userId);

        using var builder = new UpdateTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var command = new UpdateTaskCommand(task.Id, "Updated", null, null, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Priority.Should().Be(Priority.High);
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsNotFound()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);

        using var builder = new UpdateTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var otherUserId = Guid.NewGuid();
        var command = new UpdateTaskCommand(task.Id, "Hacked", null, null, null, otherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task Handle_ExistingTask_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);
        task.UpdatedAt = DateTime.UtcNow.AddDays(-1);

        using var builder = new UpdateTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var before = DateTime.UtcNow;
        var command = new UpdateTaskCommand(task.Id, "Updated", null, null, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.UpdatedAt.Should().BeOnOrAfter(before);
    }
}
