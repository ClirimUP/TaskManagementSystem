using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Users;
using TaskManagementSystem.Features.Auth.Login;
using TaskManagementSystem.Features.Auth.Register;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistence;
using Xunit;

namespace TaskManagementSystem.Tests.Auth.Handlers;

public class AuthHandlerTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher = new PasswordHasher();
    private readonly IJwtTokenService _jwtTokenService = new FakeJwtTokenService();

    public AuthHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    private async Task<User> SeedUser(string email = "test@example.com", string password = "password123")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(password),
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    // ─── Register ─────────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        var handler = new RegisterHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new RegisterCommand("user@example.com", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("user@example.com");
        result.Value.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowercase()
    {
        var handler = new RegisterHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new RegisterCommand("User@EXAMPLE.com", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        await SeedUser("existing@example.com");
        var handler = new RegisterHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new RegisterCommand("existing@example.com", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Conflict");
    }

    [Fact]
    public async Task Register_DuplicateEmailDifferentCase_ReturnsConflict()
    {
        await SeedUser("user@example.com");
        var handler = new RegisterHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new RegisterCommand("USER@EXAMPLE.COM", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Conflict");
    }

    [Fact]
    public async Task Register_PersistsUserToDatabase()
    {
        var handler = new RegisterHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new RegisterCommand("new@example.com", "password123");

        await handler.Handle(command, CancellationToken.None);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");
        user.Should().NotBeNull();
    }

    // ─── Register Validator ───────────────────────────────────────

    [Fact]
    public async Task RegisterValidator_EmptyEmail_Fails()
    {
        var validator = new RegisterValidator();
        var command = new RegisterCommand("", "password123");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterValidator_InvalidEmail_Fails()
    {
        var validator = new RegisterValidator();
        var command = new RegisterCommand("not-an-email", "password123");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task RegisterValidator_ShortPassword_Fails()
    {
        var validator = new RegisterValidator();
        var command = new RegisterCommand("user@example.com", "short");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    // ─── Login ────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        await SeedUser("login@example.com", "correctpassword");
        var handler = new LoginHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new LoginCommand("login@example.com", "correctpassword");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("login@example.com");
        result.Value.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        await SeedUser("login@example.com", "correctpassword");
        var handler = new LoginHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new LoginCommand("login@example.com", "wrongpassword");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task Login_NonExistentEmail_ReturnsUnauthorized()
    {
        var handler = new LoginHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new LoginCommand("nobody@example.com", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task Login_EmailCaseInsensitive_ReturnsSuccess()
    {
        await SeedUser("login@example.com", "password123");
        var handler = new LoginHandler(_db, _passwordHasher, _jwtTokenService);
        var command = new LoginCommand("LOGIN@EXAMPLE.COM", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    // ─── Login Validator ──────────────────────────────────────────

    [Fact]
    public async Task LoginValidator_EmptyEmail_Fails()
    {
        var validator = new LoginValidator();
        var command = new LoginCommand("", "password123");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task LoginValidator_EmptyPassword_Fails()
    {
        var validator = new LoginValidator();
        var command = new LoginCommand("user@example.com", "");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }
}

internal class FakeJwtTokenService : IJwtTokenService
{
    public string GenerateToken(User user) => $"fake-token-{user.Id}";
}
