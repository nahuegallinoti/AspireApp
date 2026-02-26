using AspireApp.Domain.ROP;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace AspireApp.Application.Implementations.Extensions;

public static class ValidateExtensions
{
    public static Result<T> Validate<T>(this T model) where T : class
    {
        ValidationContext validationContext = new(model);

        List<ValidationResult> validationResults = [];

        if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
        {
            var errors = validationResults.Select(vr => vr.ErrorMessage).ToImmutableArray();
            return Result.Failure<T>([..errors]);
        }

        return Result.Success(model);
    }
}