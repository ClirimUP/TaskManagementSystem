using MediatR;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Auth.Common;

namespace TaskManagementSystem.Features.Auth.Register;

public record RegisterCommand(
    string Email,
    string Password) : IRequest<Result<AuthResponse>>;
