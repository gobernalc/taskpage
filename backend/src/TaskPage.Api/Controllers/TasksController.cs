using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Application.Tasks.Services;

namespace TaskPage.Api.Controllers;

/// <summary>
/// Exposes the task management endpoints (create, list, get, update, delete and
/// status change).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    /// <summary>
    /// Action name MVC generates for <see cref="GetByIdAsync"/>: the framework strips
    /// the "Async" suffix, so the Location URL must target this name.
    /// </summary>
    private const string GetByIdActionName = "GetById";

    private readonly TaskService _tasks;
    private readonly IValidator<CreateTaskRequest> _createValidator;
    private readonly IValidator<UpdateTaskRequest> _updateValidator;
    private readonly IValidator<UpdateTaskStatusRequest> _statusValidator;
    private readonly ILogger<TasksController> _logger;

    /// <summary>Creates the controller with the task service, validators and logger.</summary>
    /// <param name="tasks">Task use cases.</param>
    /// <param name="createValidator">Validator for the create request.</param>
    /// <param name="updateValidator">Validator for the update request.</param>
    /// <param name="statusValidator">Validator for the status change request.</param>
    /// <param name="logger">Logger for the CRUD events.</param>
    public TasksController(
        TaskService tasks,
        IValidator<CreateTaskRequest> createValidator,
        IValidator<UpdateTaskRequest> updateValidator,
        IValidator<UpdateTaskStatusRequest> statusValidator,
        ILogger<TasksController> logger)
    {
        _tasks = tasks;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
        _logger = logger;
    }

    /// <summary>Creates a task assigned to an existing user.</summary>
    /// <param name="request">Input data to create the task.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created task with the Location header.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<TaskResponse>> CreateAsync(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        var response = await _tasks.CreateAsync(request, cancellationToken);

        _logger.LogInformation("Task created with id {TaskId}", response.Id);
        return CreatedAtAction(GetByIdActionName, new { id = response.Id }, response);
    }

    /// <summary>Lists tasks with filters, search, sorting and pagination.</summary>
    /// <param name="filter">Query filters for the listing.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged tasks matching the filter.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<TaskResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<PagedResult<TaskResponse>>> ListAsync(
        [FromQuery] TaskFilter filter,
        CancellationToken cancellationToken)
    {
        return Ok(await _tasks.ListAsync(filter, cancellationToken));
    }

    /// <summary>Gets a task by its identifier.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching task.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<TaskResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(await _tasks.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Replaces the editable values of an existing task.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Input data with the new values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated task.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<TaskResponse>> UpdateAsync(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);

        var response = await _tasks.UpdateAsync(id, request, cancellationToken);

        _logger.LogInformation("Task updated with id {TaskId}", response.Id);
        return Ok(response);
    }

    /// <summary>Deletes an existing task.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>204 No Content when the task is deleted.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _tasks.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Task deleted with id {TaskId}", id);
        return NoContent();
    }

    /// <summary>Changes the status of a task respecting the transition rules.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Input data with the new status.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated task.</returns>
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<TaskResponse>> ChangeStatusAsync(
        int id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_statusValidator, request, cancellationToken);

        var response = await _tasks.ChangeStatusAsync(id, request, cancellationToken);

        _logger.LogInformation("Task status changed for id {TaskId}", response.Id);
        return Ok(response);
    }

    /// <summary>Validates a request and throws when it is not valid.</summary>
    /// <typeparam name="T">Request type.</typeparam>
    /// <param name="validator">Validator for the request.</param>
    /// <param name="request">Request to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private static async Task ValidateAsync<T>(
        IValidator<T> validator,
        T request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
