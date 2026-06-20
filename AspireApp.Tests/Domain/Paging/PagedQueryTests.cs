using AspireApp.Domain.Paging;

namespace AspireApp.Tests.Domain.Paging;

public class PagedQueryTests
{
    private sealed class TestQuery : PagedQuery;

    [Theory]
    [InlineData(1, 25, 500, 1, 25)]
    [InlineData(0, 0, 500, 1, 25)]
    [InlineData(-2, -5, 500, 1, 25)]
    [InlineData(3, 501, 500, 3, 500)]
    [InlineData(2, 51, 50, 2, 50)]
    [InlineData(4, 20, 500, 4, 20)]
    public void NormalizeReturnsCanonicalPagination(int page, int pageSize, int maxPageSize, int expectedPage, int expectedPageSize)
    {
        var query = new TestQuery { Page = page, PageSize = pageSize };

        query.Normalize(maxPageSize).Should().Be((expectedPage, expectedPageSize));
    }
}
