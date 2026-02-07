using TaskManagementSystem.Domain.Tasks;

namespace TaskManagementSystem.Features.Tasks.Common;

public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.IsCompleted,
        task.Priority,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt);
}
