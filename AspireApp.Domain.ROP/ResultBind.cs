namespace AspireApp.Domain.ROP;

public static class ResultBindExtensions
{
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> r, Func<TIn, Result<TOut>> binder) =>
        r.Success ? binder(r.Value) : Result.Failure<TOut>(r.Errors, r.HttpStatusCode);

    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Task<Result<TOut>>> binder)
    {
        var r = await result;
        return r.Success ? await binder(r.Value) : Result.Failure<TOut>(r.Errors, r.HttpStatusCode);
    }

    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Result<TOut>> binder)
    {
        var r = await result;
        return r.Success ? binder(r.Value) : Result.Failure<TOut>(r.Errors, r.HttpStatusCode);
    }

    public static Task<Result<TOut>> Bind<TIn, TOut>(this Result<TIn> r, Func<TIn, Task<Result<TOut>>> binder) =>
        r.Success ? binder(r.Value) : Task.FromResult(Result.Failure<TOut>(r.Errors, r.HttpStatusCode));

    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> r, Func<TIn, TOut> mapper) =>
        r.Success ? Result.Success(mapper(r.Value)) : Result.Failure<TOut>(r.Errors, r.HttpStatusCode);

    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, TOut> mapper)
    {
        var r = await result;
        return r.Success ? Result.Success(mapper(r.Value)) : Result.Failure<TOut>(r.Errors, r.HttpStatusCode);
    }
}
