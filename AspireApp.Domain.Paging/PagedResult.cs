namespace AspireApp.Domain.Paging;

/// <summary>
/// Slice of a larger result set returned by paginated queries.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Total,
    int Page,
    int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);
    public bool HasPrevious => Total > 0 && Page > 1;
    public bool HasNext => Page < TotalPages;
}
