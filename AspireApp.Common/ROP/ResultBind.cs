using System.Runtime.ExceptionServices;

namespace AspireApp.Core.ROP;

public static class Result_Bind
{

    /// <summary>
    /// Allows to chain two methods, the output of the first is the input of the second.
    /// </summary>
    /// <param name="r">current Result chain</param>
    /// <param name="method">method to execute</param>
    /// <typeparam name="T">Input type</typeparam>
    /// <typeparam name="U">Output type</typeparam>
    /// <returns>Result Structure of the return type</returns>
    public static Result<U> Bind<T, U>(this Result<T> r, Func<T, Result<U>> method)
    {
        try
        {
            return r.Success
                ? method(r.Value)
                : Result.Failure<U>(r.Errors, r.HttpStatusCode);
        }
        catch (Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
            throw;
        }
    }


    /// <summary>
    /// Allows to chain two async methods, the output of the first is the input of the second.
    /// </summary>
    /// <param name="result">current Result chain</param>
    /// <param name="method">method to execute</param>
    /// <typeparam name="T">Input type</typeparam>
    /// <typeparam name="U">Output type</typeparam>
    /// <returns>Async Result Structure of the return type</returns>
    public static async Task<Result<U>> Bind<T, U>(this Task<Result<T>> result, Func<T, Task<Result<U>>> method)
    {
        try
        {
            var r = await result;
            return r.Success
                ? await method(r.Value)
                : Result.Failure<U>(r.Errors, r.HttpStatusCode);
        }
        catch (Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
            throw;
        }
    }


    /// <summary>
    /// Allows to chain an async method to a non async method, the output of the first is the input of the second.
    /// </summary>
    /// <param name="result">current Result chain</param>
    /// <param name="method">method to execute</param>
    /// <typeparam name="T">Input type</typeparam>
    /// <typeparam name="U">Output type</typeparam>
    /// <returns>Async Result Structure of the return type</returns>
    public static async Task<Result<U>> Bind<T, U>(this Task<Result<T>> result, Func<T, Result<U>> method)
    {
        try
        {
            var r = await result;

            return r.Success
                ? method(r.Value)
                : Result.Failure<U>(r.Errors, r.HttpStatusCode);
        }
        catch (Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
            throw;
        }
    }

    /// <summary>
    /// Allows to chain a synchronous method with an asynchronous method.
    /// </summary>
    /// <param name="r">Current Result chain</param>
    /// <param name="method">Asynchronous method to execute</param>
    /// <typeparam name="T">Input type</typeparam>
    /// <typeparam name="U">Output type</typeparam>
    /// <returns>Async Result Structure of the return type</returns>
    public static Task<Result<U>> Bind<T, U>(this Result<T> r, Func<T, Task<Result<U>>> method)
    {
        try
        {
            return r.Success
                ? method(r.Value)
                : Task.FromResult(Result.Failure<U>(r.Errors, r.HttpStatusCode));
        }
        catch (Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
            throw;
        }
    }

}