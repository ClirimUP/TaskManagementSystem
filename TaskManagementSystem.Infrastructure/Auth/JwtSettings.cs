namespace TaskManagementSystem.Infrastructure.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TaskManagementSystem";
    public string Audience { get; set; } = "TaskManagementSystem";
    public int ExpirationInMinutes { get; set; } = 60;
}
