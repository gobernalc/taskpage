using TaskPage.Domain.Enums;

namespace TaskPage.Application.Tasks.DTOs;

/// <summary>
/// Input data to change the status of a task.
/// </summary>
public class UpdateTaskStatusRequest
{
    /// <summary>New status for the task.</summary>
    public TaskState Status { get; set; }
}
