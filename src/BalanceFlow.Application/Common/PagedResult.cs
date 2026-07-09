namespace BalanceFlow.Application.Common;

/// <summary>
/// Wraps a page of items with pagination metadata.
/// Used by "get all" query handlers to return paginated lists.
///
/// <para><strong>Example JSON shape returned by the API:</strong></para>
/// <code>
/// {
///   "items": [ ... ],
///   "pageNumber": 1,
///   "pageSize": 10,
///   "totalCount": 42,
///   "totalPages": 5,
///   "hasPreviousPage": false,
///   "hasNextPage": true
/// }
/// </code>
/// </summary>
/// <typeparam name="T">The type of items in the page (typically a DTO).</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>The items on the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>The 1-based page number that was requested.</summary>
    public int PageNumber { get; }

    /// <summary>The maximum number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>The total number of items across all pages (before pagination).</summary>
    public int TotalCount { get; }

    /// <summary>The total number of pages, calculated from <see cref="TotalCount"/> and <see cref="PageSize"/>.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Whether there is a page before the current one.</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Whether there is a page after the current one.</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
