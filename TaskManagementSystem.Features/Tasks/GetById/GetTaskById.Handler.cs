using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.GetById;

public class GetTaskByIdHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskResponse>>
{
    private readonly AppDbContext _db;

    public GetTaskByIdHandler(AppDbContext db) => _db = db;

    public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == request.UserId, cancellationToken);

        if (task is null)
            return Result<TaskResponse>.Failure(Error.NotFound($"Task with ID '{request.Id}' was not found."));

        return Result<TaskResponse>.Success(task.ToResponse());
    }
}
