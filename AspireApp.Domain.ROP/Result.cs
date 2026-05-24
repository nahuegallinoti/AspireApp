using System.Collections.Immutable;
using System.Net;

namespace AspireApp.Domain.ROP;

public readonly struct Result<T>
{
    public T Value { get; }
    public ImmutableArray<string> Errors { get; }
    public HttpStatusCode HttpStatusCode { get; }
    public bool Success => Errors.IsDefaultOrEmpty;
    public bool IsFailure => !Success;

    public static implicit operator Result<T>(T value) => new(value, HttpStatusCode.OK);
    public static implicit operator Result<T>(ImmutableArray<string> errors) => new(errors, HttpStatusCode.BadRequest);

    public Result(T value, HttpStatusCode statusCode)
    {
        Value = value;
        Errors = ImmutableArray<string>.Empty;
        HttpStatusCode = statusCode;
    }

    public Result(ImmutableArray<string> errors, HttpStatusCode statusCode)
    {
        if (errors.IsDefaultOrEmpty)
            throw new ArgumentException("At least one error must be supplied.", nameof(errors));

        HttpStatusCode = statusCode;
        Value = default!;
        Errors = errors;
    }
}
