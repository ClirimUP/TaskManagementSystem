using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.List;

public class ListTasksHandler : IRequestHandler<ListTasksQuery, List<TaskResponse>>
{
    private readonly AppDbContext _db;

    public ListTasksHandler(AppDbContext db) => _db = db;

    public async Task<List<TaskResponse>> Handle(ListTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Tasks.AsNoTracking()
            .Where(t => t.UserId == request.UserId)
            .AsQueryable();

        query = request.Status switch
        {
            TaskStatusFilter.Active => query.Where(t => !t.IsCompleted),
            TaskStatusFilter.Completed => query.Where(t => t.IsCompleted),
            _ => query
        };

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => t.ToResponse()).ToList();
    }
}
