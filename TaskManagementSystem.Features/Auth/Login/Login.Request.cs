using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Auth.Common;

namespace TaskManagementSystem.Features.Auth.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;
