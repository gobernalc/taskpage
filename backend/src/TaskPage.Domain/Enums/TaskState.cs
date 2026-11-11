namespace TaskPage.Domain.Enums;

/// <summary>
/// Lifecycle states of a task. Persisted as their string representation.
/// </summary>
public enum TaskState
{
    /// <summary>The task has not started yet.</summary>
    Pending = 0,

    /// <summary>The task is being worked on.</summary>
    InProgress = 1,

    /// <summary>The task is finished.</summary>
    Done = 2
}
