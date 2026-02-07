using System.Security.Claims;
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
        group.MapPost("/", async (CreateTaskRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateTaskCommand(request.Title, request.Description, request.Priority, request.DueDate, userId);
            var result = await mediator.Send(command, ct);

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
