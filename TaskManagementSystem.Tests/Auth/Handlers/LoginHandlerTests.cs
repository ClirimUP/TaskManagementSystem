using FluentAssertions;
using TaskManagementSystem.Features.Auth.Login;
using TaskManagementSystem.Tests.Auth.Builders;
using TaskManagementSystem.Tests.Helpers;
using Xunit;

namespace TaskManagementSystem.Tests.Auth.Handlers;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "login@example.com", passwordHash: "hashed-pw");

        using var builder = new LoginHandlerBuilder()
            .WithExistingUser(existingUser)
            .WithPasswordVerify(returns: true)
            .WithGenerateToken("jwt-token-456");

        var handler = builder.Build();
        var command = new LoginCommand("login@example.com", "correctpassword");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token-456");
        result.Value.Email.Should().Be("login@example.com");

        builder
            .VerifyVerifyCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "login@example.com", passwordHash: "hashed-pw");

        using var builder = new LoginHandlerBuilder()
            .WithExistingUser(existingUser)
            .WithPasswordVerify(returns: false);

        var handler = builder.Build();
        var command = new LoginCommand("login@example.com", "wrongpassword");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Unauthorized");
        result.Error.Message.Should().Be("Invalid email or password.");

        builder
            .VerifyVerifyCalled()
            .VerifyGenerateTokenNotCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        using var builder = new LoginHandlerBuilder();

        var handler = builder.Build();
        var command = new LoginCommand("nobody@example.com", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Unauthorized");

        builder
            .VerifyVerifyNotCalled()
            .VerifyGenerateTokenNotCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_EmailCaseInsensitive_ReturnsSuccess()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "login@example.com", passwordHash: "hashed-pw");

        using var builder = new LoginHandlerBuilder()
            .WithExistingUser(existingUser)
            .WithPasswordVerify(returns: true)
            .WithGenerateToken("token");

        var handler = builder.Build();
        var command = new LoginCommand("LOGIN@EXAMPLE.COM", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        builder
            .VerifyVerifyCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_EmailWithWhitespace_TrimsBeforeNormalizing()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "login@example.com", passwordHash: "hashed-pw");

        using var builder = new LoginHandlerBuilder()
            .WithExistingUser(existingUser)
            .WithPasswordVerify(returns: true)
            .WithGenerateToken("token");

        var handler = builder.Build();
        var command = new LoginCommand("  login@example.com  ", "password123");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        builder
            .VerifyVerifyCalled()
            .VerifyGenerateTokenCalled()
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ValidCredentials_VerifyAndGenerateTokenCalledOnce()
    {
        // Arrange
        var existingUser = UserHelper.Generate(email: "test@example.com", passwordHash: "hashed");

        using var builder = new LoginHandlerBuilder()
            .WithExistingUser(existingUser)
            .WithPasswordVerify(returns: true)
            .WithGenerateToken("token");

        var handler = builder.Build();
        var command = new LoginCommand("test@example.com", "password");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        builder
            .VerifyVerifyCalled(times: 1)
            .VerifyGenerateTokenCalled(times: 1)
            .VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NonExistentEmail_GenerateTokenNotCalled()
    {
        // Arrange
        using var builder = new LoginHandlerBuilder();

        var handler = builder.Build();
        var command = new LoginCommand("nobody@example.com", "password123");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        builder
            .VerifyVerifyNotCalled()
            .VerifyGenerateTokenNotCalled()
            .VerifyNoOtherCalls();
    }
}
