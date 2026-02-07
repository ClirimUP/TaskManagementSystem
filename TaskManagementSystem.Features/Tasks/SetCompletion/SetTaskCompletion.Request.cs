using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.SetCompletion;

public record SetTaskCompletionCommand(Guid Id, bool IsCompleted, Guid UserId) : IRequest<Result<TaskResponse>>;

public record SetCompletionRequest(bool IsCompleted);
