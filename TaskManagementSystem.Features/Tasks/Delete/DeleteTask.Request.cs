using MediatR;
using TaskManagementSystem.Domain.Common;

namespace TaskManagementSystem.Features.Tasks.Delete;

public record DeleteTaskCommand(Guid Id) : IRequest<Result>;
