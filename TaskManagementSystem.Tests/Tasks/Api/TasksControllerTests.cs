using FluentAssertions;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Features.Tasks.Common;
using Xunit;

namespace TaskManagementSystem.Tests.Tasks.Api;

public class ResultExtensionsTests
{
    [Fact]
    public void ToProblem_NotFoundError_ReturnsResult()
    {
        // Arrange
        var result = Result.Failure(Error.NotFound("Not found"));

        // Act
        var httpResult = result.ToProblem();

        // Assert
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_ValidationError_ReturnsResult()
    {
        // Arrange
        var result = Result.Failure(Error.Validation("Invalid input"));

        // Act
        var httpResult = result.ToProblem();

        // Assert
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_ConflictError_ReturnsResult()
    {
        // Arrange
        var result = Result.Failure(Error.Conflict("Already exists"));

        // Act
        var httpResult = result.ToProblem();

        // Assert
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_UnexpectedError_ReturnsResult()
    {
        // Arrange
        var result = Result.Failure(Error.Unexpected("Something failed"));

        // Act
        var httpResult = result.ToProblem();

        // Assert
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_UnauthorizedError_ReturnsResult()
    {
        // Arrange
        var result = Result.Failure(Error.Unauthorized("Not allowed"));

        // Act
        var httpResult = result.ToProblem();

        // Assert
        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_GenericResultFailure_ReturnsResult()
    {
        // Arrange
        var result = Result<TaskResponse>.Failure(Error.NotFound("Not found"));

        // Act
        var httpResult = ((Result)result).ToProblem();

        // Assert
        httpResult.Should().NotBeNull();
    }
}
