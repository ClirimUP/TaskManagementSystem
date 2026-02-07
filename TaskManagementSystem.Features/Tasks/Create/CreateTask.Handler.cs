using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.Create;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    private readonly AppDbContext _db;

    public CreateTaskHandler(AppDbContext db) => _db = db;

    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            Priority = request.Priority ?? Priority.Medium,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(task.ToResponse());
    }
}
