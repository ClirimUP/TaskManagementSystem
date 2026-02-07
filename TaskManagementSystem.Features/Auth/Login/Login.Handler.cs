using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Auth.Common;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginHandler(AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure(Error.Unauthorized("Invalid email or password."));

        var token = _jwtTokenService.GenerateToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(token, user.Email));
    }
}
