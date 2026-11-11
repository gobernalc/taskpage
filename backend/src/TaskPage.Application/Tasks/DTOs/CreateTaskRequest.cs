using TaskPage.Domain.Enums;

namespace TaskPage.Application.Tasks.DTOs;

/// <summary>
/// Input data to create a task.
/// </summary>
public class CreateTaskRequest
{
    /// <summary>Required task title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Identifier of the assigned user.</summary>
    public int UserId { get; set; }

    /// <summary>Optional initial status; defaults to Pending when omitted.</summary>
    public TaskState? Status { get; set; }

    /// <summary>Optional additional information as a JSON document.</summary>
    public string? AdditionalData { get; set; }
}
