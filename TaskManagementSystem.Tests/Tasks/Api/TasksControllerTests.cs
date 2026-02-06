using FluentAssertions;
using MediatR;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.Api.Controllers;
using TaskManagementSystem.Domain.Common;
using TaskManagementSystem.Features.Tasks.Common;
using TaskManagementSystem.Features.Tasks.Create;
using TaskManagementSystem.Features.Tasks.Delete;
using TaskManagementSystem.Features.Tasks.GetById;
using TaskManagementSystem.Features.Tasks.List;
using TaskManagementSystem.Features.Tasks.SetCompletion;
using TaskManagementSystem.Features.Tasks.Update;

namespace TaskManagementSystem.Tests.Tasks.Api;

/// <summary>
/// Simple fake mediator that captures the request and returns a pre-configured response.
/// </summary>
public class FakeMediator : IMediator
{
    private object? _response;

    public void SetResponse<T>(T response) => _response = response;

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        => Task.FromResult((TResponse)_response!);

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
        => Task.CompletedTask;

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        => Task.FromResult(_response);

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task Publish(object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
        => Task.CompletedTask;
}

public class TasksControllerTests
{
    private readonly FakeMediator _mediator;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mediator = new FakeMediator();
        _controller = new TasksController(_mediator);
    }

    private static TaskResponse CreateTaskResponse(Guid? id = null) => new(
        id ?? Guid.NewGuid(),
        "Test Task",
        "Description",
        false,
        "Medium",
        DateTime.UtcNow.AddDays(7),
        DateTime.UtcNow,
        DateTime.UtcNow);

    // ─── List ────────────────────────────────────────────────────

    [Fact]
    public async Task List_ReturnsOkWithTaskList()
    {
        var tasks = new List<TaskResponse> { CreateTaskResponse(), CreateTaskResponse() };
        _mediator.SetResponse(tasks);

        var result = await _controller.List(null);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(tasks);
    }

    // ─── GetById ─────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingTask_ReturnsOk()
    {
        var response = CreateTaskResponse();
        _mediator.SetResponse(Result<TaskResponse>.Success(response));

        var result = await _controller.GetById(response.Id);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task GetById_NonExistingTask_Returns404()
    {
        _mediator.SetResponse(Result<TaskResponse>.Failure(Error.NotFound("Not found")));

        var result = await _controller.GetById(Guid.NewGuid());

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    // ─── Create ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidCommand_Returns201Created()
    {
        var response = CreateTaskResponse();
        _mediator.SetResponse(Result<TaskResponse>.Success(response));
        var command = new CreateTaskCommand("New Task", "Desc", "Medium", null);

        var result = await _controller.Create(command);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value.Should().Be(response);
    }

    [Fact]
    public async Task Create_ValidationFailure_Returns400()
    {
        _mediator.SetResponse(Result<TaskResponse>.Failure(Error.Validation("Title is required.")));
        var command = new CreateTaskCommand("", null, null, null);

        var result = await _controller.Create(command);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // ─── Update ──────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingTask_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var response = CreateTaskResponse(id);
        _mediator.SetResponse(Result<TaskResponse>.Success(response));

        var result = await _controller.Update(id, new UpdateTaskRequest("Updated", null, null, null));

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Update_NonExistingTask_Returns404()
    {
        _mediator.SetResponse(Result<TaskResponse>.Failure(Error.NotFound("Not found")));

        var result = await _controller.Update(Guid.NewGuid(), new UpdateTaskRequest("Title", null, null, null));

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    // ─── SetCompletion ───────────────────────────────────────────

    [Fact]
    public async Task SetCompletion_ExistingTask_ReturnsOk()
    {
        var response = CreateTaskResponse();
        _mediator.SetResponse(Result<TaskResponse>.Success(response));

        var result = await _controller.SetCompletion(response.Id, new SetCompletionRequest(true));

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task SetCompletion_NonExistingTask_Returns404()
    {
        _mediator.SetResponse(Result<TaskResponse>.Failure(Error.NotFound("Not found")));

        var result = await _controller.SetCompletion(Guid.NewGuid(), new SetCompletionRequest(true));

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    // ─── Delete ──────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingTask_Returns204NoContent()
    {
        _mediator.SetResponse(Result.Success());

        var result = await _controller.Delete(Guid.NewGuid());

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingTask_Returns404()
    {
        _mediator.SetResponse(Result.Failure(Error.NotFound("Not found")));

        var result = await _controller.Delete(Guid.NewGuid());

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
