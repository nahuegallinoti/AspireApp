using System.Collections.Immutable;
using System.Net;

namespace AspireApp.Core.ROP;

public struct Result<T>
{
    public readonly T Value;

    public static implicit operator Result<T>(T value) => new(value, HttpStatusCode.OK);

    public static implicit operator Result<T>(ImmutableArray<string> errors) => new(errors, HttpStatusCode.BadRequest);

    public readonly ImmutableArray<string> Errors;
    public readonly HttpStatusCode HttpStatusCode;
    public bool Success => Errors.Length == 0;

    public Result(T value, HttpStatusCode statusCode)
    {
        Value = value;
        Errors = [];
        HttpStatusCode = statusCode;
    }

    public Result(ImmutableArray<string> errors, HttpStatusCode statusCode)
    {
        if (errors.Length == 0)
        {
            throw new InvalidOperationException("You should specify at least one error");
        }

        HttpStatusCode = statusCode;
        Value = default!;
        Errors = errors;
    }
}
