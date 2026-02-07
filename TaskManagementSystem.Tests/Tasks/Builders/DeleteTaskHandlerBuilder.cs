using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Delete;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public class DeleteTaskHandlerBuilder : TaskHandlerBuilderBase
{
    public DeleteTaskHandlerBuilder WithExistingTask(TaskItem task)
    {
        Db.Tasks.Add(task);
        Db.SaveChanges();
        return this;
    }

    public DeleteTaskHandler Build() => new(Db);
}
