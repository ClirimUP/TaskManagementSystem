using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.GetById;

public static class GetTaskByIdEndpoint
{
    public static RouteGroupBuilder MapGetTaskById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTaskByIdQuery(id));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("GetTaskById")
        .WithSummary("Get a task by ID")
        .WithDescription("Retrieves a single task by its unique identifier.")
        .Produces<TaskResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        return group;
    }
}
