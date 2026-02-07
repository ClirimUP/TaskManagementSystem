using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.Update;

public static class UpdateTaskEndpoint
{
    public static RouteGroupBuilder MapUpdateTask(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest request, IMediator mediator) =>
        {
            var command = new UpdateTaskCommand(id, request.Title, request.Description, request.Priority, request.DueDate);
            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("UpdateTask")
        .WithSummary("Update a task")
        .WithDescription("Updates an existing task's title, description, priority, and due date.")
        .Produces<TaskResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        return group;
    }
}
