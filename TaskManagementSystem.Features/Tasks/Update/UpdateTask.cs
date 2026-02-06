using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Tasks.Update;

public record UpdateTaskCommand(
    Guid Id,
    string Title,
    string? Description,
    string? Priority,
    DateTime? DueDate) : IRequest<Result<TaskResponse>>;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task ID is required.");

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

public class UpdateTaskHandler : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
{
    private readonly AppDbContext _db;

    public UpdateTaskHandler(AppDbContext db) => _db = db;

    public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task is null)
            return Result<TaskResponse>.Failure(Error.NotFound($"Task with ID '{request.Id}' was not found."));

        var priority = string.IsNullOrEmpty(request.Priority)
            ? task.Priority
            : Enum.Parse<Priority>(request.Priority, true);

        task.Title = request.Title;
        task.Description = request.Description;
        task.Priority = priority;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<TaskResponse>.Success(task.ToResponse());
    }
}
