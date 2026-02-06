using TaskManagementSystem.Domain.Tasks;

namespace TaskManagementSystem.Features.Tasks.Common;

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    string Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.IsCompleted,
        task.Priority.ToString(),
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt);
}
