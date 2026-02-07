using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.SetCompletion;

public class SetTaskCompletionHandler : IRequestHandler<SetTaskCompletionCommand, Result<TaskResponse>>
{
    private readonly AppDbContext _db;

    public SetTaskCompletionHandler(AppDbContext db) => _db = db;

    public async Task<Result<TaskResponse>> Handle(SetTaskCompletionCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
            return Result<TaskResponse>.Failure(Error.NotFound($"Task with ID '{request.Id}' was not found."));

        if (request.IsCompleted)
            task.MarkComplete();
        else
            task.MarkIncomplete();

        await _db.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(task.ToResponse());
    }
}
