using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.List;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public class ListTasksHandlerBuilder : TaskHandlerBuilderBase
{
    public ListTasksHandlerBuilder WithExistingTask(TaskItem task)
    {
        Db.Tasks.Add(task);
        Db.SaveChanges();
        return this;
    }

    public ListTasksHandlerBuilder WithExistingTasks(IEnumerable<TaskItem> tasks)
    {
        Db.Tasks.AddRange(tasks);
        Db.SaveChanges();
        return this;
    }

    public ListTasksHandler Build() => new(Db);
}
