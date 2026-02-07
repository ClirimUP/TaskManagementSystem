using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.Create;

public static class CreateTaskEndpoint
{
    public static RouteGroupBuilder MapCreateTask(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateTaskCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Created($"/api/tasks/{result.Value!.Id}", result.Value)
                : result.ToProblem();
        })
        .WithName("CreateTask")
        .WithSummary("Create a new task")
        .WithDescription("Creates a new task with the specified title, description, priority, and optional due date.")
        .Produces<TaskResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithOpenApi();

        return group;
    }
}
