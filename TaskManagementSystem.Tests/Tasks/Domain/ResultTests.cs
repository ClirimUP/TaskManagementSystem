using FluentAssertions;
using TaskManagementSystem.Domain.Common;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResult()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ReturnsFailedResult()
    {
        var error = Error.NotFound("Not found");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ReturnsSuccessWithValue()
    {
        var value = "test value";

        var result = Result<string>.Success(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_ReturnsFailedResultWithNoValue()
    {
        var error = Error.Validation("Invalid input");

        var result = Result<string>.Failure(error);

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
        var error = Error.NotFound("Item missing");

        error.Code.Should().Be("NotFound");
        error.Message.Should().Be("Item missing");
    }

    [Fact]
    public void Validation_CreatesErrorWithValidationCode()
    {
        var error = Error.Validation("Bad input");

        error.Code.Should().Be("Validation");
        error.Message.Should().Be("Bad input");
    }

    [Fact]
    public void Conflict_CreatesErrorWithConflictCode()
    {
        var error = Error.Conflict("Already exists");

        error.Code.Should().Be("Conflict");
        error.Message.Should().Be("Already exists");
    }

    [Fact]
    public void Unexpected_CreatesErrorWithUnexpectedCode()
    {
        var error = Error.Unexpected("Something went wrong");

        error.Code.Should().Be("Unexpected");
        error.Message.Should().Be("Something went wrong");
    }

    [Fact]
    public void Unauthorized_CreatesErrorWithUnauthorizedCode()
    {
        var error = Error.Unauthorized("Not allowed");

        error.Code.Should().Be("Unauthorized");
        error.Message.Should().Be("Not allowed");
    }
}
