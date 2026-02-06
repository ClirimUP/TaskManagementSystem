using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Domain;

public class TaskItemTests
{
    [Fact]
    public void MarkComplete_SetsIsCompletedToTrue()
    {
        var task = new TaskItem { IsCompleted = false };

        task.MarkComplete();

        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void MarkComplete_UpdatesUpdatedAt()
    {
        var task = new TaskItem
        {
            IsCompleted = false,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var before = DateTime.UtcNow;

        task.MarkComplete();

        task.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkIncomplete_SetsIsCompletedToFalse()
    {
        var task = new TaskItem { IsCompleted = true };

        task.MarkIncomplete();

        task.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void MarkIncomplete_UpdatesUpdatedAt()
    {
        var task = new TaskItem
        {
            IsCompleted = true,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var before = DateTime.UtcNow;

        task.MarkIncomplete();

        task.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void NewTaskItem_HasDefaultPriorityMedium()
    {
        var task = new TaskItem();

        task.Priority.Should().Be(Priority.Medium);
    }

    [Fact]
    public void NewTaskItem_HasEmptyTitle()
    {
        var task = new TaskItem();

        task.Title.Should().BeEmpty();
    }

    [Fact]
    public void NewTaskItem_IsNotCompleted()
    {
        var task = new TaskItem();

        task.IsCompleted.Should().BeFalse();
    }
}
