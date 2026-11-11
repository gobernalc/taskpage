using TaskPage.Domain.Enums;

namespace TaskPage.Application.Tasks.DTOs;

/// <summary>
/// Query filters for listing tasks with filters, search, sorting and
/// pagination. Values are normalized before being passed to the repository.
/// </summary>
public class TaskFilter
{
    private const int MinPage = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
    {
        "title",
        "status",
        "createdat"
    };

    /// <summary>Filters tasks by status.</summary>
    public TaskState? Status { get; set; }

    /// <summary>Filters tasks by assigned user.</summary>
    public int? UserId { get; set; }

    /// <summary>Filters tasks by the priority value inside AdditionalData.</summary>
    public string? Priority { get; set; }

    /// <summary>Free text matched against the task title.</summary>
    public string? Search { get; set; }

    /// <summary>Field used for sorting.</summary>
    public string? Sort { get; set; }

    /// <summary>Sort direction: asc or desc.</summary>
    public string? Order { get; set; }

    /// <summary>One-based page index.</summary>
    public int Page { get; set; } = MinPage;

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>Applies defaults and clamps the values to their valid ranges.</summary>
    /// <returns>The same instance with normalized values.</returns>
    public TaskFilter Normalize()
    {
        Page = Math.Max(MinPage, Page);
        PageSize = Math.Clamp(PageSize, MinPageSize, MaxPageSize);

        var sort = (Sort ?? "createdat").Trim();
        Sort = AllowedSorts.Contains(sort) ? sort.ToLowerInvariant() : "createdat";

        var order = (Order ?? "asc").Trim();
        Order = order.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

        Search = string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
        Priority = string.IsNullOrWhiteSpace(Priority) ? null : Priority.Trim().ToLowerInvariant();
        return this;
    }
}
