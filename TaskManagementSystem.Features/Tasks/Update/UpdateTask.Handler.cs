using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.Update;

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
{
    private readonly AppDbContext _db;

    public UpdateTaskHandler(AppDbContext db) => _db = db;

    public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);

        if (task is null)
            return Result<TaskResponse>.Failure(Error.NotFound($"Task with ID '{request.Id}' was not found."));

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = request.Priority ?? task.Priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(task.ToResponse());
    }
}
