using TaskPage.Application.Common.Models;
using TaskPage.Application.Users.DTOs;
using TaskPage.Domain.Entities;

namespace TaskPage.Application.Common.Interfaces;

/// <summary>
/// Persistence contract for users. Implemented by the infrastructure layer.
/// </summary>
public interface IUserRepository
{
    /// <summary>Gets a user by its identifier.</summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user or null when it does not exist.</returns>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a user by its already-normalized email.</summary>
    /// <param name="email">Email normalized with trim and lowercase.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching user or null when it does not exist.</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of users applying search, sorting and pagination.</summary>
    /// <param name="filter">Normalized filter with search, sort, order and paging values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged users matching the filter.</returns>
    Task<PagedResult<User>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken = default);

    /// <summary>Counts the tasks assigned to a user.</summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Number of tasks assigned to the user.</returns>
    Task<int> CountTasksByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new user.</summary>
    /// <param name="user">User to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing user.</summary>
    /// <param name="user">User with the updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Deletes an existing user.</summary>
    /// <param name="user">User to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}
