using FluentAssertions;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Features.Tasks.Update;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Validators;

public class UpdateTaskValidatorTests
{
    private readonly UpdateTaskValidator _validator = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new UpdateTaskCommand(Guid.NewGuid(), "Valid Title", "Description", Priority.Low, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyId_Fails()
    {
        // Arrange
        var command = new UpdateTaskCommand(Guid.Empty, "Valid Title", null, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public async Task Validate_EmptyTitle_Fails()
    {
        // Arrange
        var command = new UpdateTaskCommand(Guid.NewGuid(), "", null, null, null, _userId);

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
        var command = new UpdateTaskCommand(Guid.NewGuid(), "Ab", null, null, null, _userId);

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
        var command = new UpdateTaskCommand(Guid.NewGuid(), longTitle, null, null, null, _userId);

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
        var command = new UpdateTaskCommand(Guid.NewGuid(), "Valid Title", longDescription, null, null, _userId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
