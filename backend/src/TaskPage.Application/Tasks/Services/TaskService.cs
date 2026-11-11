using TaskPage.Application.Common.Interfaces;
using TaskPage.Application.Common.Models;
using TaskPage.Application.Tasks.DTOs;
using TaskPage.Domain.Entities;
using TaskPage.Domain.Enums;
using TaskPage.Domain.Exceptions;

namespace TaskPage.Application.Tasks.Services;

/// <summary>
/// Implements the use cases related to tasks, including the state
/// transition rules.
/// </summary>
public class TaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IUserRepository _users;

    /// <summary>Creates the service with its repository dependencies.</summary>
    /// <param name="tasks">Task persistence contract.</param>
    /// <param name="users">User persistence contract.</param>
    public TaskService(ITaskRepository tasks, IUserRepository users)
    {
        _tasks = tasks;
        _users = users;
    }

    /// <summary>Creates a task assigned to an existing user.</summary>
    /// <param name="request">Input data to create the task.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The created task.</returns>
    /// <exception cref="NotFoundException">The assigned user does not exist.</exception>
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with id {request.UserId} was not found.");

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Status = request.Status ?? TaskState.Pending,
            UserId = request.UserId,
            User = user,
            AdditionalData = NormalizeAdditionalData(request.AdditionalData),
            CreatedAt = DateTime.UtcNow
        };

        await _tasks.AddAsync(task, cancellationToken);
        return Map(task);
    }

    /// <summary>Gets a task by its identifier.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching task.</returns>
    /// <exception cref="NotFoundException">The task does not exist.</exception>
    public async Task<TaskResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Task with id {id} was not found.");

        return Map(task);
    }

    /// <summary>Lists tasks applying filters, search, sorting and pagination.</summary>
    /// <param name="filter">Query filters, normalized inside the method.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Paged tasks matching the filter.</returns>
    public async Task<PagedResult<TaskResponse>> ListAsync(TaskFilter filter, CancellationToken cancellationToken = default)
    {
        filter.Normalize();

        var result = await _tasks.GetPagedAsync(filter, cancellationToken);

        return new PagedResult<TaskResponse>
        {
            Items = result.Items.Select(Map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    /// <summary>Replaces the editable values of an existing task.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Input data with the new values.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated task.</returns>
    /// <exception cref="NotFoundException">The task or the assigned user does not exist.</exception>
    public async Task<TaskResponse> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Task with id {id} was not found.");

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User with id {request.UserId} was not found.");

        task.Title = request.Title.Trim();
        task.UserId = request.UserId;
        task.User = user;
        task.AdditionalData = NormalizeAdditionalData(request.AdditionalData);
        task.UpdatedAt = DateTime.UtcNow;

        await _tasks.UpdateAsync(task, cancellationToken);
        return Map(task);
    }

    /// <summary>Deletes an existing task.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="NotFoundException">The task does not exist.</exception>
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Task with id {id} was not found.");

        await _tasks.DeleteAsync(task, cancellationToken);
    }

    /// <summary>Changes the status of a task respecting the transition rules.</summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Input data with the new status.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated task.</returns>
    /// <exception cref="NotFoundException">The task does not exist.</exception>
    /// <exception cref="BusinessRuleException">The transition Pending to Done is forbidden.</exception>
    public async Task<TaskResponse> ChangeStatusAsync(int id, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Task with id {id} was not found.");

        if (task.Status == TaskState.Pending && request.Status == TaskState.Done)
        {
            throw new BusinessRuleException("A task cannot transition directly from Pending to Done.");
        }

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _tasks.UpdateAsync(task, cancellationToken);
        return Map(task);
    }

    /// <summary>Trims additional data or returns null when it is blank.</summary>
    /// <param name="value">Raw additional data.</param>
    /// <returns>Normalized additional data or null.</returns>
    private static string? NormalizeAdditionalData(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Maps a task entity to its response DTO.</summary>
    /// <param name="task">Task to map.</param>
    /// <returns>Task response.</returns>
    private static TaskResponse Map(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Status = task.Status,
        UserId = task.UserId,
        UserName = task.User.Name,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
        AdditionalData = task.AdditionalData
    };
}
