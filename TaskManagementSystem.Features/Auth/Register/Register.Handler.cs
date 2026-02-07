using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Domain.Users;
using TaskManagementSystem.Features.Auth.Common;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Features.Auth.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterHandler(AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _db.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            return Result<AuthResponse>.Failure(Error.Conflict("A user with this email already exists."));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(token, user.Email));
    }
}
