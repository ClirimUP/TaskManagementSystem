using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Create;
using TaskManagementSystem.Tests.Tasks.Builders;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Handlers;

public class CreateTaskHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithMappedFields()
    {
        // Arrange
        using var builder = new CreateTaskHandlerBuilder();
        var handler = builder.Build();
        var command = new CreateTaskCommand("New Task", "A description", Priority.High, DateTime.UtcNow.AddDays(5), _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("New Task");
        result.Value.Description.Should().Be("A description");
        result.Value.Priority.Should().Be(Priority.High);
        result.Value.IsCompleted.Should().BeFalse();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_NullPriority_DefaultsToMedium()
    {
        // Arrange
        using var builder = new CreateTaskHandlerBuilder();
        var handler = builder.Build();
        var command = new CreateTaskCommand("Task", null, null, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsToDatabase()
    {
        // Arrange
        using var builder = new CreateTaskHandlerBuilder();
        var handler = builder.Build();
        var db = builder.GetDbContext();
        var command = new CreateTaskCommand("Persisted Task", "desc", Priority.Low, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await db.Tasks.FindAsync(result.Value!.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Persisted Task");
        saved.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsTimestampsAndIsCompletedFalse()
    {
        // Arrange
        using var builder = new CreateTaskHandlerBuilder();
        var handler = builder.Build();
        var before = DateTime.UtcNow;
        var command = new CreateTaskCommand("Task", null, null, null, _userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value!.IsCompleted.Should().BeFalse();
        result.Value.CreatedAt.Should().BeOnOrAfter(before);
        result.Value.UpdatedAt.Should().BeOnOrAfter(before);
    }
}
