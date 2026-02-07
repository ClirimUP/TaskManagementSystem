using FluentAssertions;
using TaskManagementSystem.Features.Auth.Register;
using Xunit;

namespace TaskManagementSystem.Tests.Auth.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyEmail_Fails()
    {
        // Arrange
        var command = new RegisterCommand("", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_InvalidEmailFormat_Fails()
    {
        // Arrange
        var command = new RegisterCommand("not-an-email", "password123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_EmailTooLong_Fails()
    {
        // Arrange
        var longEmail = new string('a', 248) + "@test.com";
        var command = new RegisterCommand(longEmail, "password123");

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
        var command = new RegisterCommand("user@example.com", "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_PasswordTooShort_Fails()
    {
        // Arrange
        var command = new RegisterCommand("user@example.com", "short");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_PasswordTooLong_Fails()
    {
        // Arrange
        var longPassword = new string('a', 129);
        var command = new RegisterCommand("user@example.com", longPassword);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
