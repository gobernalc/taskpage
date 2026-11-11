namespace TaskPage.Application.Common.Models;

/// <summary>
/// Generic paged result returned by list operations.
/// </summary>
/// <typeparam name="T">Type of the items in the page.</typeparam>
public class PagedResult<T>
{
    /// <summary>Items of the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>One-based index of the current page.</summary>
    public int Page { get; init; }

    /// <summary>Requested page size.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalItems { get; init; }

    /// <summary>Total number of available pages.</summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}
