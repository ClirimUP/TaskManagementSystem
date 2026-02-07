using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Create;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public class CreateTaskHandlerBuilder : TaskHandlerBuilderBase
{
    public CreateTaskHandler Build() => new(Db);
}
