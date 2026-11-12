using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Users.DTOs;
using TaskPage.Application.Users.Services;

namespace TaskPage.Api.Controllers;

/// <summary>
/// Exposes the user management endpoints (create, list, get, update and delete).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Action name MVC generates for <see cref="GetByIdAsync"/>: the framework strips
    /// the "Async" suffix, so the Location URL must target this name.
    /// </summary>
    private const string GetByIdActionName = "GetById";

    private readonly UserService _users;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly ILogger<UsersController> _logger;

    /// <summary>Creates the controller with the user service, validators and logger.</summary>
    /// <param name="users">User use cases.</param>
    /// <param name="createValidator">Validator for the create request.</param>
    /// <param name="updateValidator">Validator for the update request.</param>
    /// <param name="logger">Logger for the CRUD events.</param>
    public UsersController(
        UserService users,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        ILogger<UsersController> logger)
    {
        _users = users;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    /// <summary>Creates a new user.</summary>
    /// <param name="request">Input data to create the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created user with the Location header.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<UserResponse>> CreateAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        var response = await _users.CreateAsync(request, cancellationToken);

        _logger.LogInformation("User created with id {UserId}", response.Id);
        return CreatedAtAction(GetByIdActionName, new { id = response.Id }, response);
    }

    /// <summary>Lists users with search, sorting and pagination.</summary>
    /// <param name="filter">Query filters for the listing.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged users matching the filter.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<UserResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<PagedResult<UserResponse>>> ListAsync(
        [FromQuery] UserFilter filter,
        CancellationToken cancellationToken)
    {
        return Ok(await _users.ListAsync(filter, cancellationToken));
    }

    /// <summary>Gets a user by its identifier.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<UserResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return Ok(await _users.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Replaces the name and email of an existing user.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="request">Input data with the new values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<UserResponse>> UpdateAsync(
        int id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateAsync(_updateValidator, request, cancellationToken);

        var response = await _users.UpdateAsync(id, request, cancellationToken);

        _logger.LogInformation("User updated with id {UserId}", response.Id);
        return Ok(response);
    }

    /// <summary>Deletes a user with no associated tasks.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>204 No Content when the user is deleted.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _users.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("User deleted with id {UserId}", id);
        return NoContent();
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
