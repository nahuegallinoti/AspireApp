using System.Net;
using AspireApp.Domain.ROP;

namespace AspireApp.Tests.Domain.ROP;

public class ResultTests
{
    [Fact]
    public void Success_should_carry_value_and_ok_status()
    {
        var r = 42.Success();

        r.Success.Should().BeTrue();
        r.IsFailure.Should().BeFalse();
        r.Value.Should().Be(42);
        r.HttpStatusCode.Should().Be(HttpStatusCode.OK);
        r.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_should_carry_errors_and_default_to_bad_request()
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
    public void Failure_should_respect_explicit_status(string error, HttpStatusCode status)
    {
        var r = Result.Failure<int>(error, status);

        r.HttpStatusCode.Should().Be(status);
    }

    [Fact]
    public void Bind_should_chain_on_success_and_short_circuit_on_failure()
    {
        var ok = 1.Success().Bind(x => Result.Success(x + 1));
        ok.Success.Should().BeTrue();
        ok.Value.Should().Be(2);

        var bad = Result.Failure<int>("nope").Bind(x => Result.Success(x + 1));
        bad.Success.Should().BeFalse();
        bad.Errors.Should().Contain("nope");
    }

    [Fact]
    public async Task Bind_async_should_propagate_status_on_failure()
    {
        var r = await Task.FromResult(Result.Failure<int>("nope", HttpStatusCode.Conflict))
            .Bind(_ => Task.FromResult(Result.Success("ok")));

        r.Success.Should().BeFalse();
        r.HttpStatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
