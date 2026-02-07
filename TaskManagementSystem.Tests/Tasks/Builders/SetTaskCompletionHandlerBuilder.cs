using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.SetCompletion;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public class SetTaskCompletionHandlerBuilder : TaskHandlerBuilderBase
{
    public SetTaskCompletionHandlerBuilder WithExistingTask(TaskItem task)
    {
        Db.Tasks.Add(task);
        Db.SaveChanges();
        return this;
    }

    public SetTaskCompletionHandler Build() => new(Db);
}
