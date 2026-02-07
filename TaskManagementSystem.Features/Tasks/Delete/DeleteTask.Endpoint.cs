using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Common;

namespace TaskManagementSystem.Features.Tasks.Delete;

public static class DeleteTaskEndpoint
{
    public static RouteGroupBuilder MapDeleteTask(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeleteTaskCommand(id), ct);

            return result.IsSuccess
                ? Results.NoContent()
                : result.ToProblem();
        })
        .WithName("DeleteTask")
        .WithSummary("Delete a task")
        .WithDescription("Permanently deletes a task by its unique identifier.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        return group;
    }
}
