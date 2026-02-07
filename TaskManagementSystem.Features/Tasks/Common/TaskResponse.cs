using TaskManagementSystem.Domain.Tasks;

namespace TaskManagementSystem.Features.Tasks.Common;

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    Priority Priority,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt);
