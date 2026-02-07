using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Update;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public class UpdateTaskHandlerBuilder : TaskHandlerBuilderBase
{
    public UpdateTaskHandlerBuilder WithExistingTask(TaskItem task)
    {
        Db.Tasks.Add(task);
        Db.SaveChanges();
        return this;
    }

    public UpdateTaskHandler Build() => new(Db);
}
