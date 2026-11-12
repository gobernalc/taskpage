using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Domain.Entities;

namespace TaskPage.Api.Tests.Fakes;

/// <summary>
/// In-memory fake of the task persistence contract used by the API tests.
/// </summary>
internal sealed class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = [];
    private int _nextId = 1;

    /// <summary>Exposes the stored tasks for assertions.</summary>
    public IReadOnlyList<TaskItem> Tasks => _tasks;

    /// <summary>Seeds a task without assigning an identifier.</summary>
    /// <param name="task">Task to store.</param>
    public void Seed(TaskItem task) => _tasks.Add(task);

    /// <inheritdoc />
    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

    /// <inheritdoc />
    public Task<PagedResult<TaskItem>> GetPagedAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        var items = _tasks
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<TaskItem>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = _tasks.Count
        });
    }

    /// <inheritdoc />
    public Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        if (task.Id == 0)
        {
            task.Id = _nextId++;
        }

        _tasks.Add(task);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default) => Task.CompletedTask;

    /// <inheritdoc />
    public Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _tasks.Remove(task);
        return Task.CompletedTask;
    }
}
