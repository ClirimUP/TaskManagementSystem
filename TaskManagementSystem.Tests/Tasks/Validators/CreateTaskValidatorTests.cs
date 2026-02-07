using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Create;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Validators;

public class CreateTaskValidatorTests
{
    private readonly CreateTaskValidator _validator = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new CreateTaskCommand("Valid Title", "Description", Priority.High, DateTime.UtcNow, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyTitle_Fails()
    {
        // Arrange
        var command = new CreateTaskCommand("", null, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_TitleTooShort_Fails()
    {
        // Arrange
        var command = new CreateTaskCommand("Ab", null, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_TitleTooLong_Fails()
    {
        // Arrange
        var longTitle = new string('a', 121);
        var command = new CreateTaskCommand(longTitle, null, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_Fails()
    {
        // Arrange
        var longDescription = new string('a', 2001);
        var command = new CreateTaskCommand("Valid Title", longDescription, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_InvalidPriorityEnum_Fails()
    {
        // Arrange
        var command = new CreateTaskCommand("Valid Title", null, (Priority)999, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Priority");
    }

    [Fact]
    public async Task Validate_NullPriority_Passes()
    {
        // Arrange
        var command = new CreateTaskCommand("Valid Title", null, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
