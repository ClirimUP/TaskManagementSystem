using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.GetById;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public class GetTaskByIdHandlerBuilder : TaskHandlerBuilderBase
{
    public GetTaskByIdHandlerBuilder WithExistingTask(TaskItem task)
    {
        Db.Tasks.Add(task);
        Db.SaveChanges();
        return this;
    }

    public GetTaskByIdHandler Build() => new(Db);
}
