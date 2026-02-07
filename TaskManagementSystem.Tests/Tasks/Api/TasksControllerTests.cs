using FluentAssertions;
using Xunit;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Features.Common;

namespace TaskManagementSystem.Tests.Tasks.Api;

public class ResultExtensionsTests
{
    [Fact]
    public void ToProblem_NotFoundError_ReturnsResult()
    {
        var result = Result.Failure(Error.NotFound("Not found"));

        var httpResult = result.ToProblem();

        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_ValidationError_ReturnsResult()
    {
        var result = Result.Failure(Error.Validation("Invalid input"));

        var httpResult = result.ToProblem();

        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_ConflictError_ReturnsResult()
    {
        var result = Result.Failure(Error.Conflict("Already exists"));

        var httpResult = result.ToProblem();

        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_UnexpectedError_ReturnsResult()
    {
        var result = Result.Failure(Error.Unexpected("Something failed"));

        var httpResult = result.ToProblem();

        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_UnauthorizedError_ReturnsResult()
    {
        var result = Result.Failure(Error.Unauthorized("Not allowed"));

        var httpResult = result.ToProblem();

        httpResult.Should().NotBeNull();
    }

    [Fact]
    public void ToProblem_GenericResultFailure_ReturnsResult()
    {
        var result = Result<TaskResponse>.Failure(Error.NotFound("Not found"));

        var httpResult = ((Result)result).ToProblem();

        httpResult.Should().NotBeNull();
    }
}
