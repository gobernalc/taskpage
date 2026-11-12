using System.Text.Json;
using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Domain.Entities;

namespace TaskPage.Application.Tests.Fakes;

/// <summary>
/// In-memory fake of the task persistence contract used by the application tests.
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
        IEnumerable<TaskItem> query = _tasks;

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.UserId.HasValue)
        {
            query = query.Where(t => t.UserId == filter.UserId.Value);
        }

        if (filter.Priority is not null)
        {
            query = query.Where(t => ReadPriority(t.AdditionalData) == filter.Priority);
        }

        if (filter.Search is not null)
        {
            query = query.Where(t => t.Title.Contains(filter.Search));
        }

        query = filter.Sort switch
        {
            "status" => filter.Order == "desc"
                ? query.OrderByDescending(t => t.Status)
                : query.OrderBy(t => t.Status),
            "title" => filter.Order == "desc"
                ? query.OrderByDescending(t => t.Title)
                : query.OrderBy(t => t.Title),
            _ => filter.Order == "desc"
                ? query.OrderByDescending(t => t.CreatedAt)
                : query.OrderBy(t => t.CreatedAt)
        };

        var totalItems = query.Count();
        var items = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<TaskItem>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = totalItems
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

    /// <summary>Reads the priority property from the additional data JSON, mirroring JSON_VALUE.</summary>
    /// <param name="additionalData">Additional data JSON document.</param>
    /// <returns>Lowercased priority value or null.</returns>
    private static string? ReadPriority(string? additionalData)
    {
        if (string.IsNullOrWhiteSpace(additionalData))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(additionalData);
            return document.RootElement.TryGetProperty("priority", out var property)
                ? property.GetString()?.ToLowerInvariant()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
