using FluentAssertions;
using TaskManagementSystem.Features.Auth.Login;
using Xunit;

namespace TaskManagementSystem.Tests.Auth.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyEmail_Fails()
    {
        // Arrange
        var command = new LoginCommand("", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_EmptyPassword_Fails()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
