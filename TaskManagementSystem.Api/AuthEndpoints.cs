using TaskManagementSystem.Features.Auth.Login;
using TaskManagementSystem.Features.Auth.Register;

namespace TaskManagementSystem.Api;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group
            .MapRegister()
            .MapLogin();

        return app;
    }
}
