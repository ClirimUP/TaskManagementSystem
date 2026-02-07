using MediatR;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.List;

public record ListTasksQuery(TaskStatusFilter? Status, Guid UserId) : IRequest<List<TaskResponse>>;
