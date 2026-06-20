using System.Net;
using AspireApp.Domain.ROP;

namespace AspireApp.Tests.Domain.ROP;

public class ResultBindExtensionsTests
{
    [Fact]
    public void SynchronousBindExecutesBinderForSuccess()
    {
        var result = Result.Success(2).Bind(x => Result.Success(x * 3));

        result.Success.Should().BeTrue();
        result.Value.Should().Be(6);
    }

    [Fact]
    public void SynchronousBindDoesNotExecuteBinderForFailure()
    {
        var executed = false;
        var source = Result.NotFound<int>("missing");

        var result = source.Bind(x => { executed = true; return Result.Success(x.ToString()); });

        executed.Should().BeFalse();
        AssertFailurePreserved(result);
    }

    [Fact]
    public async Task TaskToAsyncBindHandlesSuccessAndFailure()
    {
        var success = await Task.FromResult(Result.Success(2)).Bind(x => Task.FromResult(Result.Success(x * 2)));
        var executed = false;
        var failure = await Task.FromResult(Result.NotFound<int>("missing")).Bind(x =>
        {
            executed = true;
            return Task.FromResult(Result.Success(x * 2));
        });

        success.Value.Should().Be(4);
        executed.Should().BeFalse();
        AssertFailurePreserved(failure);
    }

    [Fact]
    public async Task TaskToSynchronousBindHandlesSuccessAndFailure()
    {
        var success = await Task.FromResult(Result.Success(2)).Bind(x => Result.Success(x * 2));
        var executed = false;
        var failure = await Task.FromResult(Result.NotFound<int>("missing")).Bind(x =>
        {
            executed = true;
            return Result.Success(x * 2);
        });

        success.Value.Should().Be(4);
        executed.Should().BeFalse();
        AssertFailurePreserved(failure);
    }

    [Fact]
    public async Task ResultToAsyncBindHandlesSuccessAndFailure()
    {
        var success = await Result.Success(2).Bind(x => Task.FromResult(Result.Success(x * 2)));
        var executed = false;
        var failure = await Result.NotFound<int>("missing").Bind(x =>
        {
            executed = true;
            return Task.FromResult(Result.Success(x * 2));
        });

        success.Value.Should().Be(4);
        executed.Should().BeFalse();
        AssertFailurePreserved(failure);
    }

    [Fact]
    public void MapTransformsSuccessAndSkipsFailure()
    {
        Result.Success(2).Map(x => x.ToString()).Value.Should().Be("2");
        var executed = false;

        var failure = Result.NotFound<int>("missing").Map(x => { executed = true; return x.ToString(); });

        executed.Should().BeFalse();
        AssertFailurePreserved(failure);
    }

    [Fact]
    public async Task AsyncMapTransformsSuccessAndPreservesFailure()
    {
        var success = await Task.FromResult(Result.Success(2)).Map(x => x * 3);
        var failure = await Task.FromResult(Result.NotFound<int>("missing")).Map(x => x * 3);

        success.Value.Should().Be(6);
        AssertFailurePreserved(failure);
    }

    [Fact]
    public void BindAndMapChainProducesFinalValue()
    {
        Result.Success(2).Bind(x => Result.Success(x + 1)).Map(x => x * 4).Value.Should().Be(12);
    }

    private static void AssertFailurePreserved<T>(Result<T> result)
    {
        result.Errors.Should().Equal("missing");
        result.HttpStatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
