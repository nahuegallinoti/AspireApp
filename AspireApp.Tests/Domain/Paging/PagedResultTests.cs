using AspireApp.Domain.Paging;

namespace AspireApp.Tests.Domain.Paging;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 1, 3, 0, false, false)]
    [InlineData(10, 1, 3, 4, false, true)]
    [InlineData(10, 2, 3, 4, true, true)]
    [InlineData(10, 4, 3, 4, true, false)]
    [InlineData(10, 1, 0, 0, false, false)]
    [InlineData(10, 2, -1, 0, true, false)]
    public void CalculatedPropertiesReflectPagination(int total, int page, int pageSize, int totalPages, bool hasPrevious, bool hasNext)
    {
        var result = new PagedResult<int>([], total, page, pageSize);

        result.TotalPages.Should().Be(totalPages);
        result.HasPrevious.Should().Be(hasPrevious);
        result.HasNext.Should().Be(hasNext);
    }
}
