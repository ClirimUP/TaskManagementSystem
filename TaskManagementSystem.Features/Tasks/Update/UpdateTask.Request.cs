using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.Update;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    Priority? Priority,
    DateTime? DueDate,
    Guid UserId) : IRequest<Result<TaskResponse>>;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    Priority? Priority,
    DateTime? DueDate);
