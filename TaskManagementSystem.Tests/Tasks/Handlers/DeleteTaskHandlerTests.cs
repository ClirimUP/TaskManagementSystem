using FluentAssertions;
using TaskManagementSystem.Features.Tasks.Delete;
using TaskManagementSystem.Tests.Helpers;
using TaskManagementSystem.Tests.Tasks.Builders;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class DeleteTaskHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ExistingTask_ReturnsSuccess()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);

        using var builder = new DeleteTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new DeleteTaskCommand(task.Id, _userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingTask_RemovesFromDatabase()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);

        using var builder = new DeleteTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var db = builder.GetDbContext();

        // Act
        await handler.Handle(new DeleteTaskCommand(task.Id, _userId), CancellationToken.None);

        // Assert
        var deleted = await db.Tasks.FindAsync(task.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        using var builder = new DeleteTaskHandlerBuilder();
        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new DeleteTaskCommand(Guid.NewGuid(), _userId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsNotFound()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);

        using var builder = new DeleteTaskHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var otherUserId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new DeleteTaskCommand(task.Id, otherUserId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }
}
