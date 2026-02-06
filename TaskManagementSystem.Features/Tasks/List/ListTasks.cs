using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.List;

public record ListTasksQuery(string? Status) : IRequest<List<TaskResponse>>;

public class ListTasksHandler : IRequestHandler<ListTasksQuery, List<TaskResponse>>
{
    private readonly AppDbContext _db;

    public ListTasksHandler(AppDbContext db) => _db = db;

    public async Task<List<TaskResponse>> Handle(ListTasksQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Tasks.AsNoTracking().AsQueryable();

        query = request.Status?.ToLowerInvariant() switch
        {
            "active" => query.Where(t => !t.IsCompleted),
            "completed" => query.Where(t => t.IsCompleted),
            _ => query // "all" or null — return everything
        };

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        return tasks.Select(t => t.ToResponse()).ToList();
    }
}
