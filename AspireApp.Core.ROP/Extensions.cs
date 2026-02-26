using System.Collections.Immutable;
using System.Net;

namespace AspireApp.Domain.ROP;

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
    public static Result<T> Success<T>(this T value) => new(value, HttpStatusCode.OK);

    /// <summary>
    /// chains an object into the Result Structure
    /// </summary>
    public static Result<T> Success<T>(this T value, HttpStatusCode httpStatusCode) => new(value, httpStatusCode);

    /// <summary>
    /// chains an Result.Unit into the Result Structure
    /// </summary>
    public static Result<Unit> Success() => new(Unit, HttpStatusCode.OK);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(ImmutableArray<string> errors) => new(errors, HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(ImmutableArray<string> errors, HttpStatusCode httpStatusCode) => new(errors, httpStatusCode);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(string error) => new([error], HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<T> Failure<T>(string error, HttpStatusCode httpStatusCode) => new([error], httpStatusCode);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(ImmutableArray<string> errors) => new(errors, HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(ImmutableArray<string> errors, HttpStatusCode httpStatusCode) => new(errors, httpStatusCode);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(IEnumerable<string> errors) =>new(ImmutableArray.Create(errors.ToArray()), HttpStatusCode.BadRequest);

    /// <summary>
    /// Converts the type into the error flow with  HttpStatusCode.BadRequest
    /// </summary>
    public static Result<Unit> Failure(string error) => new([error], HttpStatusCode.BadRequest);
}
