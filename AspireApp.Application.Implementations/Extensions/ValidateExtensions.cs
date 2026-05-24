using System.ComponentModel.DataAnnotations;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Implementations.Extensions;

public static class ValidateExtensions
{
    public static Result<T> Validate<T>(this T model) where T : class
    {
        var context = new ValidationContext(model);
        List<ValidationResult> results = [];

        if (Validator.TryValidateObject(model, context, results, validateAllProperties: true))
            return Result.Success(model);

        return Result.Failure<T>(results.Select(r => r.ErrorMessage ?? "Validation error."));
    }
}
