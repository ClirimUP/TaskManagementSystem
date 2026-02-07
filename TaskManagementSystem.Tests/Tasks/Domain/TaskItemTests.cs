using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Domain;

public class TaskItemTests
{
    [Fact]
    public void MarkComplete_SetsIsCompletedToTrue()
    {
        // Arrange
        var task = new TaskItem { IsCompleted = false };

        // Act
        task.MarkComplete();

        // Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void MarkComplete_UpdatesUpdatedAt()
    {
        // Arrange
        var task = new TaskItem
        {
            IsCompleted = false,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var before = DateTime.UtcNow;

        // Act
        task.MarkComplete();

        // Assert
        task.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkIncomplete_SetsIsCompletedToFalse()
    {
        // Arrange
        var task = new TaskItem { IsCompleted = true };

        // Act
        task.MarkIncomplete();

        // Assert
        task.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void MarkIncomplete_UpdatesUpdatedAt()
    {
        // Arrange
        var task = new TaskItem
        {
            IsCompleted = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var before = DateTime.UtcNow;

        // Act
        task.MarkIncomplete();

        // Assert
        task.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void NewTaskItem_HasDefaultPriorityMedium()
    {
        // Arrange & Act
        var task = new TaskItem();

        // Assert
        task.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public void NewTaskItem_HasEmptyTitle()
    {
        // Arrange & Act
        var task = new TaskItem();

        // Assert
        task.Title.Should().BeEmpty();
    }

    [Fact]
    public void NewTaskItem_IsNotCompleted()
    {
        // Arrange & Act
        var task = new TaskItem();

        // Assert
        task.IsCompleted.Should().BeFalse();
    }
}
