using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Features.Tasks.Common;

namespace TaskManagementSystem.Features.Tasks.SetCompletion;

public static class SetTaskCompletionEndpoint
{
    public static RouteGroupBuilder MapSetTaskCompletion(this RouteGroupBuilder group)
    {
        group.MapPatch("/{id:guid}/complete", async (Guid id, SetCompletionRequest request, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new SetTaskCompletionCommand(id, request.IsCompleted, userId);
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("SetTaskCompletion")
        .WithSummary("Set task completion status")
        .WithDescription("Marks a task as complete or incomplete.")
        .Produces<TaskResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi();

        return group;
    }
}
