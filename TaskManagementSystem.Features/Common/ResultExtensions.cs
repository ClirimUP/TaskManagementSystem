using Microsoft.AspNetCore.Http;
using TaskManagementSystem.Domain.Common;

namespace TaskManagementSystem.Features.Common;

public static class ResultExtensions
{
    public static IResult ToProblem(this Result result)
    {
        var error = result.Error!;

        var statusCode = error.Code switch
        {
            "NotFound" => StatusCodes.Status404NotFound,
            "Validation" => StatusCodes.Status400BadRequest,
            "Conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: statusCode);
    }
}
