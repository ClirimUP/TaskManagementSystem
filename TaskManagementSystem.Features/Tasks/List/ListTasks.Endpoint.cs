using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.List;

public static class ListTasksEndpoint
{
    public static RouteGroupBuilder MapListTasks(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (TaskStatusFilter? status, IMediator mediator) =>
        {
            var result = await mediator.Send(new ListTasksQuery(status));
            return Results.Ok(result);
        })
        .WithName("ListTasks")
        .WithSummary("List all tasks")
        .WithDescription("Retrieves all tasks, optionally filtered by completion status.")
        .Produces<List<TaskResponse>>()
        .WithOpenApi();

        return group;
    }
}
