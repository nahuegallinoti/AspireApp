namespace AspireApp.Domain.Paging;

public enum SortDirection
{
    Asc,
    Desc,
}

/// <summary>
/// Generic envelope for paginated/sorted queries. Lives in a Domain-level shared
/// project so inner ports (data-access interfaces) can reference it without knowing
/// about application-level DTOs. Concrete per-entity filter DTOs (outer layers)
/// inherit from this and add their own typed fields.
/// </summary>
public abstract class PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    /// <summary>Name of the property to sort by (matches a property on the entity/model).</summary>
    public string? SortBy { get; set; }

    public SortDirection SortDir { get; set; } = SortDirection.Asc;

    /// <summary>Clamps to safe bounds and returns the canonical (page, pageSize) tuple.</summary>
    public (int Page, int PageSize) Normalize(int maxPageSize = 500)
    {
        var size = PageSize <= 0 ? 25 : Math.Min(PageSize, maxPageSize);
        var page = Page <= 0 ? 1 : Page;
        return (page, size);
    }
}
