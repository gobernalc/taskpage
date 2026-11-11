namespace TaskPage.Application.Tasks.DTOs;

/// <summary>
/// Input data to replace the editable values of an existing task.
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>Required task title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Identifier of the assigned user.</summary>
    public int UserId { get; set; }

    /// <summary>Additional information as a JSON document; null clears the field.</summary>
    public string? AdditionalData { get; set; }
}
