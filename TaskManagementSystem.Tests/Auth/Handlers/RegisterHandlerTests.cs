using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Features.Auth.Register;
using TaskManagementSystem.Tests.Auth.Builders;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Auth.Handlers;

public class RegisterHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithTokenAndEmail()
    {
        // Arrange
        using var builder = new RegisterHandlerBuilder()
            .WithPasswordHash("hashed-password")
            .WithGenerateToken("jwt-token-123");

        var handler = builder.Build();
        var command = new RegisterCommand("User@Example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token-123");
        result.Value.Email.Should().Be("user@example.com");

        builder
            .VerifyHashCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_EmailWithMixedCase_NormalizesToLowercase()
    {
        // Arrange
        using var builder = new RegisterHandlerBuilder()
            .WithPasswordHash("hashed")
            .WithGenerateToken("token");

        var handler = builder.Build();
        var command = new RegisterCommand("USER@EXAMPLE.COM", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("user@example.com");

        builder
            .VerifyHashCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_EmailWithWhitespace_TrimsBeforeNormalizing()
    {
        // Arrange
        using var builder = new RegisterHandlerBuilder()
            .WithPasswordHash("hashed")
            .WithGenerateToken("token");

        var handler = builder.Build();
        var command = new RegisterCommand("  user@example.com  ", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("user@example.com");

        builder
            .VerifyHashCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "existing@example.com");

        using var builder = new RegisterHandlerBuilder()
            .WithExistingUser(existingUser);

        var handler = builder.Build();
        var command = new RegisterCommand("existing@example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Conflict");
        result.Error.Message.Should().Be("A user with this email already exists.");

        builder
            .VerifyHashNotCalled()
            .VerifyGenerateTokenNotCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_DuplicateEmailDifferentCase_ReturnsConflict()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "user@example.com");

        using var builder = new RegisterHandlerBuilder()
            .WithExistingUser(existingUser);

        var handler = builder.Build();
        var command = new RegisterCommand("USER@EXAMPLE.COM", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Conflict");

        builder
            .VerifyHashNotCalled()
            .VerifyGenerateTokenNotCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsUserWithHashedPassword()
    {
        // Arrange
        using var builder = new RegisterHandlerBuilder()
            .WithPasswordHash("bcrypt-hashed-value")
            .WithGenerateToken("token");

        var handler = builder.Build();
        var db = builder.GetDbContext();
        var command = new RegisterCommand("new@example.com", "password123");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "new@example.com");
        user.Should().NotBeNull();
        user!.PasswordHash.Should().Be("bcrypt-hashed-value");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        builder
            .VerifyHashCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsHashOnceAndGenerateTokenOnce()
    {
        // Arrange
        using var builder = new RegisterHandlerBuilder()
            .WithPasswordHash("hashed")
            .WithGenerateToken("token");

        var handler = builder.Build();
        var command = new RegisterCommand("test@example.com", "password123");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        builder
            .VerifyHashCalled(times: 1)
            .VerifyGenerateTokenCalled(times: 1)
            .VerifyNoOtherCalls();
    }
}
