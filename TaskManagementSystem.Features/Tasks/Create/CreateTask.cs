using FluentValidation;
using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.Create;

public record CreateTaskCommand(
    string Title,
    string? Description,
    string? Priority,
    DateTime? DueDate) : IRequest<Result<TaskResponse>>;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
            .MaximumLength(120).WithMessage("Title must not exceed 120 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Priority)
            .Must(p => p is null || Enum.TryParse<Priority>(p, true, out _))
            .WithMessage("Priority must be Low, Medium, or High.");
    }
}

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    private readonly AppDbContext _db;

    public CreateTaskHandler(AppDbContext db) => _db = db;

    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var priority = string.IsNullOrEmpty(request.Priority)
            ? Priority.Medium
            : Enum.Parse<Priority>(request.Priority, true);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            Priority = priority,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(task.ToResponse());
    }
}
