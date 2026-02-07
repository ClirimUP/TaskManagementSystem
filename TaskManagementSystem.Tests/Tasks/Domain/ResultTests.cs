using FluentAssertions;
using TaskManagementSystem.Domain.Common;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResult()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ReturnsFailedResult()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ReturnsSuccessWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_ReturnsFailedResultWithNoValue()
    {
        // Arrange
        var error = Error.Validation("Invalid input");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }
}

public class ErrorTests
{
    [Fact]
    public void NotFound_CreatesErrorWithNotFoundCode()
    {
        // Arrange & Act
        var error = Error.NotFound("Item missing");

        // Assert
        error.Code.Should().Be("NotFound");
        error.Message.Should().Be("Item missing");
    }

    [Fact]
    public void Validation_CreatesErrorWithValidationCode()
    {
        // Arrange & Act
        var error = Error.Validation("Bad input");

        // Assert
        error.Code.Should().Be("Validation");
        error.Message.Should().Be("Bad input");
    }

    [Fact]
    public void Conflict_CreatesErrorWithConflictCode()
    {
        // Arrange & Act
        var error = Error.Conflict("Already exists");

        // Assert
        error.Code.Should().Be("Conflict");
        error.Message.Should().Be("Already exists");
    }

    [Fact]
    public void Unexpected_CreatesErrorWithUnexpectedCode()
    {
        // Arrange & Act
        var error = Error.Unexpected("Something went wrong");

        // Assert
        error.Code.Should().Be("Unexpected");
        error.Message.Should().Be("Something went wrong");
    }

    [Fact]
    public void Unauthorized_CreatesErrorWithUnauthorizedCode()
    {
        // Arrange & Act
        var error = Error.Unauthorized("Not allowed");

        // Assert
        error.Code.Should().Be("Unauthorized");
        error.Message.Should().Be("Not allowed");
    }
}
