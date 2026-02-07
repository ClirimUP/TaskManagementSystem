using FluentAssertions;
using TaskManagementSystem.Features.Tasks.GetById;
using TaskManagementSystem.Tests.Helpers;
using TaskManagementSystem.Tests.Tasks.Builders;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class GetTaskByIdHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ExistingTask_ReturnsSuccess()
    {
        // Arrange
        var task = TaskItemHelper.Generate(title: "Test Task", userId: _userId);

        using var builder = new GetTaskByIdHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();

        // Act
        var result = await handler.Handle(new GetTaskByIdQuery(task.Id, _userId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(task.Id);
        result.Value.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task Handle_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        using var builder = new GetTaskByIdHandlerBuilder();
        var handler = builder.Build();
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new GetTaskByIdQuery(nonExistentId, _userId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
        result.Error.Message.Should().Contain(nonExistentId.ToString());
    }

    [Fact]
    public async Task Handle_DifferentUser_ReturnsNotFound()
    {
        // Arrange
        var task = TaskItemHelper.Generate(userId: _userId);

        using var builder = new GetTaskByIdHandlerBuilder()
            .WithExistingTask(task);

        var handler = builder.Build();
        var otherUserId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(new GetTaskByIdQuery(task.Id, otherUserId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("NotFound");
    }
}
