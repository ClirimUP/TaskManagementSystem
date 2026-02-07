using FluentAssertions;
using TaskManagementSystem.Features.Tasks.SetCompletion;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Validators;

public class SetTaskCompletionValidatorTests
{
    private readonly SetTaskCompletionValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_Passes()
    {
        // Arrange
        var command = new SetTaskCompletionCommand(Guid.NewGuid(), true, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyId_Fails()
    {
        // Arrange
        var command = new SetTaskCompletionCommand(Guid.Empty, true, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
