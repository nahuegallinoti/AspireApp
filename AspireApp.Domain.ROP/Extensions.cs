using System.Collections.Immutable;
using System.Net;

namespace AspireApp.Domain.ROP;

public static class Result
{
    public static readonly Unit Unit = Unit.Value;

    public static Result<T> Success<T>(this T value) => new(value, HttpStatusCode.OK);
    public static Result<T> Success<T>(this T value, HttpStatusCode httpStatusCode) => new(value, httpStatusCode);
    public static Result<Unit> Success() => new(Unit, HttpStatusCode.OK);

    public static Result<T> Failure<T>(ImmutableArray<string> errors, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) =>
        new(errors, httpStatusCode);

    public static Result<T> Failure<T>(string error, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) =>
        new([error], httpStatusCode);

    public static Result<T> Failure<T>(IEnumerable<string> errors, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) =>
        new([.. errors], httpStatusCode);

    public static Result<Unit> Failure(string error, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) =>
        new([error], httpStatusCode);

    public static Result<Unit> Failure(IEnumerable<string> errors, HttpStatusCode httpStatusCode = HttpStatusCode.BadRequest) =>
        new([.. errors], httpStatusCode);

    public static Result<T> NotFound<T>(string error = "Resource not found.") =>
        new([error], HttpStatusCode.NotFound);

    public static Result<T> Conflict<T>(string error) => new([error], HttpStatusCode.Conflict);
    public static Result<T> Unauthorized<T>(string error = "Unauthorized.") => new([error], HttpStatusCode.Unauthorized);
}
