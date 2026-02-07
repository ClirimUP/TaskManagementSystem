using Bogus;
using TaskManagementSystem.Domain.Tasks;

namespace TaskManagementSystem.Tests.Helpers;

public static class TaskItemHelper
{
    private static readonly Faker<TaskItem> Faker = new Faker<TaskItem>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Title, f => f.Lorem.Sentence(3))
        .RuleFor(x => x.Description, f => f.Lorem.Paragraph())
        .RuleFor(x => x.IsCompleted, false)
        .RuleFor(x => x.Priority, f => f.PickRandom<Priority>())
        .RuleFor(x => x.DueDate, f => f.Date.Future())
        .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
        .RuleFor(x => x.UpdatedAt, (f, t) => t.CreatedAt)
        .RuleFor(x => x.UserId, f => f.Random.Guid());

    public static TaskItem Generate(
        Guid? id = null,
        string? title = null,
        string? description = null,
        bool? isCompleted = null,
        Priority? priority = null,
        DateTime? dueDate = null,
        Guid? userId = null)
    {
        var task = Faker.Generate();

        if (id.HasValue) task.Id = id.Value;
        if (title is not null) task.Title = title;
        if (description is not null) task.Description = description;
        if (isCompleted.HasValue) task.IsCompleted = isCompleted.Value;
        if (priority.HasValue) task.Priority = priority.Value;
        if (dueDate.HasValue) task.DueDate = dueDate.Value;
        if (userId.HasValue) task.UserId = userId.Value;

        return task;
    }

    public static List<TaskItem> GenerateMany(int count, Guid? userId = null)
    {
        return Enumerable.Range(0, count)
            .Select(_ => Generate(userId: userId))
            .ToList();
    }
}
