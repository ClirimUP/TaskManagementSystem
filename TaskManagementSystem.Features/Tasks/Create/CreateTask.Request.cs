using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.Create;

public record CreateTaskCommand(
    string Title,
    string? Description,
    Priority? Priority,
    DateTime? DueDate,
    Guid UserId) : IRequest<Result<TaskResponse>>;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Priority? Priority,
    DateTime? DueDate);
