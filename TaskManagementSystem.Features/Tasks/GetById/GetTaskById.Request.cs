using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.GetById;

public record GetTaskByIdQuery(Guid Id) : IRequest<Result<TaskResponse>>;
