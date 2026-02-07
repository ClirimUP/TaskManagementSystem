using FluentAssertions;
using TaskManagementSystem.Features.Tasks.SetCompletion;
using TaskManagementSystem.Tests.Helpers;
using TaskManagementSystem.Tests.Tasks.Builders;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class SetTaskCompletionHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_MarkComplete_SetsIsCompletedTrue()
    {
        // Arrange
        var task = TaskItemHelper.Generate(isCompleted: false, userId: _userId);

        using var builder = new SetTaskCompletionHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new SetTaskCompletionCommand(task.Id, true, _userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MarkIncomplete_SetsIsCompletedFalse()
    {
        // Arrange
        var task = TaskItemHelper.Generate(isCompleted: true, userId: _userId);

        using var builder = new SetTaskCompletionHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new SetTaskCompletionCommand(task.Id, false, _userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        using var builder = new SetTaskCompletionHandlerBuilder();
        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new SetTaskCompletionCommand(Guid.NewGuid(), true, _userId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsNotFound()
    {
        // Arrange
        var task = TaskItemHelper.Generate(isCompleted: false, userId: _userId);

        using var builder = new SetTaskCompletionHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var otherUserId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new SetTaskCompletionCommand(task.Id, true, otherUserId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }
}
