using TaskManagementSystem.Domain.Users;

namespace TaskManagementSystem.Infrastructure.Auth;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
