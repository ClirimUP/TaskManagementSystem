using FluentAssertions;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Features.Tasks.List;
using TaskManagementSystem.Tests.Helpers;
using TaskManagementSystem.Tests.Tasks.Builders;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class ListTasksHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_NoFilter_ReturnsAllUserTasks()
    {
        // Arrange
        var task1 = TaskItemHelper.Generate(title: "Task 1", isCompleted: false, userId: _userId);
        var task2 = TaskItemHelper.Generate(title: "Task 2", isCompleted: true, userId: _userId);

        using var builder = new ListTasksHandlerBuilder()
            .WithExistingTask(task1)
            .WithExistingTask(task2);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new ListTasksQuery(null, _userId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ActiveFilter_ReturnsOnlyIncompleteTasks()
    {
        // Arrange
        var activeTask = TaskItemHelper.Generate(title: "Active", isCompleted: false, userId: _userId);
        var completedTask = TaskItemHelper.Generate(title: "Completed", isCompleted: true, userId: _userId);

        using var builder = new ListTasksHandlerBuilder()
            .WithExistingTask(activeTask)
            .WithExistingTask(completedTask);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new ListTasksQuery(TaskStatusFilter.Active, _userId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Active");
        result[0].IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_CompletedFilter_ReturnsOnlyCompletedTasks()
    {
        // Arrange
        var activeTask = TaskItemHelper.Generate(title: "Active", isCompleted: false, userId: _userId);
        var completedTask = TaskItemHelper.Generate(title: "Completed", isCompleted: true, userId: _userId);

        using var builder = new ListTasksHandlerBuilder()
            .WithExistingTask(activeTask)
            .WithExistingTask(completedTask);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new ListTasksQuery(TaskStatusFilter.Completed, _userId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Completed");
        result[0].IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AllFilterExplicit_ReturnsAllUserTasks()
    {
        // Arrange
        var task1 = TaskItemHelper.Generate(isCompleted: false, userId: _userId);
        var task2 = TaskItemHelper.Generate(isCompleted: true, userId: _userId);

        using var builder = new ListTasksHandlerBuilder()
            .WithExistingTask(task1)
            .WithExistingTask(task2);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new ListTasksQuery(TaskStatusFilter.All, _userId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsEmpty()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);

        using var builder = new ListTasksHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var otherUserId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new ListTasksQuery(null, otherUserId), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
