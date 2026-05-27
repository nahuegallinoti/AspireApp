using AspireApp.Domain.ROP;
using Microsoft.AspNetCore.Mvc;

namespace AspireApp.Api.Infrastructure;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.Success)
            return onSuccess?.Invoke(result.Value) ?? controller.Ok(result.Value);

        return controller.Problem(
            detail: string.Join("; ", result.Errors),
            statusCode: (int)result.HttpStatusCode,
            title: result.HttpStatusCode.ToString());
    }
}
