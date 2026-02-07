using TaskManagementSystem.Domain.Users;

namespace TaskManagementSystem.Domain.Tasks;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public void MarkComplete()
    {
        IsCompleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkIncomplete()
    {
        IsCompleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
