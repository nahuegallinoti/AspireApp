using System.Net;
using AspireApp.Domain.ROP;

namespace AspireApp.Tests.Domain.ROP;

public class ResultTests
{
    [Fact]
    public void SuccessShouldCarryValueAndOkStatus()
    {
        var r = 42.Success();

        r.Success.Should().BeTrue();
        r.IsFailure.Should().BeFalse();
        r.Value.Should().Be(42);
        r.HttpStatusCode.Should().Be(HttpStatusCode.OK);
        r.Errors.Should().BeEmpty();
    }

    [Fact]
    public void FailureShouldCarryErrorsAndDefaultToBadRequest()
    {
        var r = Result.Failure<int>("oops");

        r.Success.Should().BeFalse();
        r.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        r.Errors.Should().ContainSingle().Which.Should().Be("oops");
    }

    [Theory]
    [InlineData("Not found", HttpStatusCode.NotFound)]
    [InlineData("Conflict", HttpStatusCode.Conflict)]
    [InlineData("Unauthorized", HttpStatusCode.Unauthorized)]
    public void FailureShouldRespectExplicitStatus(string error, HttpStatusCode status)
    {
        var r = Result.Failure<int>(error, status);

        r.HttpStatusCode.Should().Be(status);
    }

    [Fact]
    public void BindShouldChainOnSuccessAndShortCircuitOnFailure()
    {
        var ok = 1.Success().Bind(x => Result.Success(x + 1));
        ok.Success.Should().BeTrue();
        ok.Value.Should().Be(2);

        var bad = Result.Failure<int>("nope").Bind(x => Result.Success(x + 1));
        bad.Success.Should().BeFalse();
        bad.Errors.Should().Contain("nope");
    }

    [Fact]
    public async Task BindAsyncShouldPropagateStatusOnFailure()
    {
        var r = await Task.FromResult(Result.Failure<int>("nope", HttpStatusCode.Conflict))
            .Bind(_ => Task.FromResult(Result.Success("ok")));

        r.Success.Should().BeFalse();
        r.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
