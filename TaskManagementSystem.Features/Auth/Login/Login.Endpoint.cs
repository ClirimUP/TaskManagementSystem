using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TaskManagementSystem.Features.Auth.Common;
using TaskManagementSystem.Features.Common;

namespace TaskManagementSystem.Features.Auth.Login;

public static class LoginEndpoint
{
    public static RouteGroupBuilder MapLogin(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (LoginCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ToProblem();
        })
        .WithName("Login")
        .WithSummary("Log in to an existing account")
        .WithDescription("Authenticates a user and returns a JWT token.")
        .Produces<AuthResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithOpenApi();

        return group;
    }
}
