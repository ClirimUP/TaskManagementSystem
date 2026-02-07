using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Auth.Common;
using TaskManagementSystem.Features.Common;

namespace TaskManagementSystem.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static RouteGroupBuilder MapRegister(this RouteGroupBuilder group)
    {
        group.MapPost("/register", async (RegisterCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account and returns a JWT token.")
        .Produces<AuthResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithOpenApi();

        return group;
    }
}
