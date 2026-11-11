using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Domain.Entities;

namespace TaskPage.Application.Common.Interfaces;

/// <summary>
/// Persistence contract for tasks. Implemented by the infrastructure layer.
/// Read operations must include the assigned <see cref="User"/> navigation.
/// </summary>
public interface ITaskRepository
{
    /// <summary>Gets a task by its identifier, including its assigned user.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching task or null when it does not exist.</returns>
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of tasks applying filters, search, sorting and pagination.</summary>
    /// <param name="filter">Normalized filter with status, user, priority, search, sort and paging values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged tasks matching the filter, each one with its assigned user.</returns>
    Task<PagedResult<TaskItem>> GetPagedAsync(TaskFilter filter, CancellationToken cancellationToken = default);

    /// <summary>Adds a new task.</summary>
    /// <param name="task">Task to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing task.</summary>
    /// <param name="task">Task with the updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>Deletes an existing task.</summary>
    /// <param name="task">Task to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default);
}
