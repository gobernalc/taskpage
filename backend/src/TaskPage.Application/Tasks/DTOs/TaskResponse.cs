using TaskPage.Domain.Enums;

namespace TaskPage.Application.Tasks.DTOs;

/// <summary>
/// Read-only representation of a task returned by the API.
/// </summary>
public class TaskResponse
{
    /// <summary>Task identifier.</summary>
    public int Id { get; init; }

    /// <summary>Task title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Current status of the task.</summary>
    public TaskState Status { get; init; }

    /// <summary>Identifier of the assigned user.</summary>
    public int UserId { get; init; }

    /// <summary>Name of the assigned user.</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>Creation timestamp in UTC.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>Last modification timestamp in UTC, when applicable.</summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>Additional information stored as a JSON document.</summary>
    public string? AdditionalData { get; init; }
}
