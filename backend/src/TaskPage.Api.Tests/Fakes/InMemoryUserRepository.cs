using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Users.DTOs;
using TaskPage.Domain.Entities;

namespace TaskPage.Api.Tests.Fakes;

/// <summary>
/// In-memory fake of the user persistence contract used by the API tests.
/// </summary>
internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    private readonly Dictionary<int, int> _taskCounts = [];
    private int _nextId = 1;

    /// <summary>Exposes the stored users for assertions.</summary>
    public IReadOnlyList<User> Users => _users;

    /// <summary>Seeds a user without assigning an identifier.</summary>
    /// <param name="user">User to store.</param>
    public void Seed(User user) => _users.Add(user);

    /// <summary>Configures the number of tasks associated with a user.</summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="count">Task count returned by the repository.</param>
    public void SetTaskCount(int userId, int count) => _taskCounts[userId] = count;

    /// <inheritdoc />
    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    /// <inheritdoc />
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    /// <inheritdoc />
    public Task<PagedResult<User>> GetPagedAsync(UserFilter filter, CancellationToken cancellationToken = default)
    {
        var items = _users
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<User>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = _users.Count
        });
    }

    /// <inheritdoc />
    public Task<int> CountTasksByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_taskCounts.GetValueOrDefault(userId));

    /// <inheritdoc />
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.Id == 0)
        {
            user.Id = _nextId++;
        }

        _users.Add(user);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _users.Remove(user);
        return Task.CompletedTask;
    }
}
