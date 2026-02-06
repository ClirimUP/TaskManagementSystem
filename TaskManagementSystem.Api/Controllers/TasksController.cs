using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Features.Tasks.Create;
using TaskManagementSystem.Features.Tasks.Delete;
using TaskManagementSystem.Features.Tasks.GetById;
using TaskManagementSystem.Features.Tasks.List;
using TaskManagementSystem.Features.Tasks.SetCompletion;
using TaskManagementSystem.Features.Tasks.Update;

namespace TaskManagementSystem.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Retrieves all tasks, optionally filtered by status.
    /// </summary>
    /// <param name="status">Filter: all, active, or completed</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var result = await _mediator.Send(new ListTasksQuery(status));
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single task by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id));
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
            return ToProblem(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand(id, request.Title, request.Description, request.Priority, request.DueDate);
        var result = await _mediator.Send(command);
        return ToActionResult(result);
    }

    /// <summary>
    /// Marks a task as complete or incomplete.
    /// </summary>
    [HttpPatch("{id:guid}/complete")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCompletion(Guid id, [FromBody] SetCompletionRequest request)
    {
        var command = new SetTaskCompletionCommand(id, request.IsCompleted);
        var result = await _mediator.Send(command);
        return ToActionResult(result);
    }

    /// <summary>
    /// Deletes a task by its ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTaskCommand(id));

        if (result.IsFailure)
            return ToProblem(result);

        return NoContent();
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return ToProblem(result);
    }

    private IActionResult ToProblem(Result result)
    {
        var error = result.Error!;

        var statusCode = error.Code switch
        {
            "NotFound" => StatusCodes.Status404NotFound,
            "Validation" => StatusCodes.Status400BadRequest,
            "Conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Code,
            Detail = error.Message,
            Type = statusCode switch
            {
                400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                409 => "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                _ => "https://tools.ietf.org/html/rfc9110#section-15.6.1"
            }
        };

        return StatusCode(statusCode, problemDetails);
    }
}

// Request DTOs for endpoints that receive data from the body separate from route params
public record UpdateTaskRequest(string Title, string? Description, string? Priority, DateTime? DueDate);
public record SetCompletionRequest(bool IsCompleted);
