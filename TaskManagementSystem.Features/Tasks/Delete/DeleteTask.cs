using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.Delete;

public record DeleteTaskCommand(Guid Id) : IRequest<Result>;

public class DeleteTaskHandler : IRequestHandler<DeleteTaskCommand, Result>
{
    private readonly AppDbContext _db;

    public DeleteTaskHandler(AppDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
            return Result.Failure(Error.NotFound($"Task with ID '{request.Id}' was not found."));

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
