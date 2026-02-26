using System.Collections.Immutable;
using System.Net;

namespace AspireApp.Domain.ROP;

public readonly struct Result<T>
{
    public readonly T Value;
    public readonly ImmutableArray<string> Errors;
    public readonly HttpStatusCode HttpStatusCode;
    public bool Success => Errors.Length is 0;

    public static implicit operator Result<T>(T value) => new(value, HttpStatusCode.OK);
    public static implicit operator Result<T>(ImmutableArray<string> errors) => new(errors, HttpStatusCode.BadRequest);

    public Result(T value, HttpStatusCode statusCode)
    {
        Value = value;
        Errors = [];
        HttpStatusCode = statusCode;
    }

    public Result(ImmutableArray<string> errors, HttpStatusCode statusCode)
    {
        if (errors.Length is 0)
            throw new ArgumentException("Debe especificarse al menos un error", nameof(errors));

        HttpStatusCode = statusCode;
        Value = default!;
        Errors = errors;
    }
}
