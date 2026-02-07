using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Mappings;

public class TaskMappingsTests
{
    [Fact]
    public void ToResponse_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var task = TaskItemHelper.Generate(
            id: Guid.Parse("11111111-1111-1111-1111-111111111111"),
            title: "Test Title",
            description: "Test Description",
            isCompleted: true,
            priority: Priority.High,
            userId: Guid.NewGuid());
        task.DueDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        task.CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        task.UpdatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = task.ToResponse();

        // Assert
        response.Id.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        response.Title.Should().Be("Test Title");
        response.Description.Should().Be("Test Description");
        response.IsCompleted.Should().BeTrue();
        response.Priority.Should().Be(Priority.High);
        response.DueDate.Should().Be(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc));
        response.CreatedAt.Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        response.UpdatedAt.Should().Be(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
