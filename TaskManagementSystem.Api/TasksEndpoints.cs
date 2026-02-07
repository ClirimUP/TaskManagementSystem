using TaskManagementSystem.Features.Tasks.Create;
using TaskManagementSystem.Features.Tasks.Delete;
using TaskManagementSystem.Features.Tasks.GetById;
using TaskManagementSystem.Features.Tasks.List;
using TaskManagementSystem.Features.Tasks.SetCompletion;
using TaskManagementSystem.Features.Tasks.Update;

namespace TaskManagementSystem.Api;

public static class TasksEndpoints
{
    public static WebApplication MapTasksEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks")
            .WithTags("Tasks")
            .RequireAuthorization();

        group
            .MapListTasks()
            .MapGetTaskById()
            .MapCreateTask()
            .MapUpdateTask()
            .MapSetTaskCompletion()
            .MapDeleteTask();

        return app;
    }
}
