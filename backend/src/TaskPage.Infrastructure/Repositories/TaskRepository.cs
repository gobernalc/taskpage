using Microsoft.EntityFrameworkCore;
using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Domain.Entities;
using TaskPage.Infrastructure.Persistence;

namespace TaskPage.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the task persistence contract. The priority
/// filter reads the AdditionalData JSON with JSON_VALUE.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private const string PriorityPath = "$.priority";

    private readonly AppDbContext _context;

    /// <summary>Creates the repository with the database context.</summary>
    /// <param name="context">EF Core database context.</param>
    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Gets a task by its identifier, including its assigned user.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching task or null when it does not exist.</returns>
    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>Gets a page of tasks applying filters, search, sorting and pagination.</summary>
    /// <param name="filter">Normalized filter with status, user, priority, search, sort and paging values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged tasks matching the filter, each one with its assigned user.</returns>
    public async Task<PagedResult<TaskItem>> GetPagedAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .AsNoTracking()
            .Include(t => t.User);

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
            query = query.Where(t =>
                SqlServerJsonFunctions.JsonValue(t.AdditionalData, PriorityPath) == filter.Priority);
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

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskItem>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = totalItems
        };
    }

    /// <summary>Adds a new task.</summary>
    /// <param name="task">Task to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Updates an existing task.</summary>
    /// <param name="task">Task with the updated values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Deletes an existing task.</summary>
    /// <param name="task">Task to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task DeleteAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
