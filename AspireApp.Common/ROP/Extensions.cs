using System.Collections.Immutable;
using System.Net;

namespace AspireApp.Core.ROP;

// La clase debe llamarse Result para que funcione
public static class Result
{
    /// <summary>
    /// Object to avoid using void
    /// </summary>
    public static readonly Unit Unit = Unit.Value;

    /// <summary>
    /// chains an object into the Result Structure
    /// </summary>
    public static Result<T> Success<T>(this T value) => new Result<T>(value, HttpStatusCode.OK);

    /// <summary>
    /// chains an object into the Result Structure
    /// </summary>
    public static Result<T> Success<T>(this T value, HttpStatusCode httpStatusCode) => new Result<T>(value, httpStatusCode);

    /// <summary>
    /// chains an Result.Unit into the Result Structure
    /// </summary>
    public static Result<Unit> Success() => new Result<Unit>(Unit, HttpStatusCode.OK);

    /// <summary>
    /// Converts a synchronous Result structure into async
    /// </summary>
    public static Task<Result<T>> Async<T>(this Result<T> r) => Task.FromResult(r);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(ImmutableArray<string> errors) => new(errors, HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(ImmutableArray<string> errors, HttpStatusCode httpStatusCode) => new Result<T>(errors, httpStatusCode);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(string error) => new Result<T>([error], HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(ImmutableArray<string> errors) => new Result<Unit>(errors, HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(ImmutableArray<string> errors, HttpStatusCode httpStatusCode) => new Result<Unit>(errors, httpStatusCode);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(IEnumerable<string> errors) =>
        new Result<Unit>(ImmutableArray.Create(errors.ToArray()), HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(string error) => new Result<Unit>(ImmutableArray.Create(error), HttpStatusCode.BadRequest);
}
