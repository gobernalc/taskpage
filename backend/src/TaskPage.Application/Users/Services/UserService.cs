using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Users.DTOs;
using TaskPage.Domain.Entities;
using TaskPage.Domain.Exceptions;

namespace TaskPage.Application.Users.Services;

/// <summary>
/// Implements the use cases related to users, including the email
/// normalization and uniqueness rules.
/// </summary>
public class UserService
{
    private readonly IUserRepository _users;

    /// <summary>Creates the service with its repository dependency.</summary>
    /// <param name="users">User persistence contract.</param>
    public UserService(IUserRepository users)
    {
        _users = users;
    }

    /// <summary>Creates a new user with a normalized and unique email.</summary>
    /// <param name="request">Input data to create the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created user.</returns>
    /// <exception cref="ConflictException">The email is already registered.</exception>
    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var existing = await _users.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("The email address is already registered.");
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        await _users.AddAsync(user, cancellationToken);
        return Map(user);
    }

    /// <summary>Gets a user by its identifier.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user.</returns>
    /// <exception cref="NotFoundException">The user does not exist.</exception>
    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"User with id {id} was not found.");

        return Map(user);
    }

    /// <summary>Lists users applying search, sorting and pagination.</summary>
    /// <param name="filter">Query filters, normalized inside the method.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged users matching the filter.</returns>
    public async Task<PagedResult<UserResponse>> ListAsync(UserFilter filter, CancellationToken cancellationToken = default)
    {
        filter.Normalize();

        var result = await _users.GetPagedAsync(filter, cancellationToken);

        return new PagedResult<UserResponse>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    /// <summary>Replaces the name and email of an existing user.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="request">Input data with the new values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated user.</returns>
    /// <exception cref="NotFoundException">The user does not exist.</exception>
    /// <exception cref="ConflictException">The email belongs to another user.</exception>
    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"User with id {id} was not found.");

        var email = NormalizeEmail(request.Email);

        var existing = await _users.GetByEmailAsync(email, cancellationToken);
        if (existing is not null && existing.Id != id)
        {
            throw new ConflictException("The email address is already registered.");
        }

        user.Name = request.Name.Trim();
        user.Email = email;
        user.UpdatedAt = DateTime.UtcNow;

        await _users.UpdateAsync(user, cancellationToken);
        return Map(user);
    }

    /// <summary>Deletes a user that has no associated tasks.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="NotFoundException">The user does not exist.</exception>
    /// <exception cref="ConflictException">The user has associated tasks.</exception>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"User with id {id} was not found.");

        var taskCount = await _users.CountTasksByUserIdAsync(id, cancellationToken);
        if (taskCount > 0)
        {
            throw new ConflictException("The user cannot be deleted because it has associated tasks.");
        }

        await _users.DeleteAsync(user, cancellationToken);
    }

    /// <summary>Normalizes an email by trimming and lowercasing it.</summary>
    /// <param name="email">Raw email address.</param>
    /// <returns>Normalized email.</returns>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Maps a user entity to its response DTO.</summary>
    /// <param name="user">User to map.</param>
    /// <returns>User response.</returns>
    private static UserResponse Map(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
