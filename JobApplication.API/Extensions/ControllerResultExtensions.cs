using JobApplication.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Extensions
{
    public static class ControllerResultExtensions
    {
        // Failed Result -> 404 / 403 / 400 with the same { errors } body AuthController uses.
        public static IActionResult FailureResult<T>(this ControllerBase controller, Result<T> result)
        {
            var body = new { errors = result.Errors };

            return result.Kind switch
            {
                ErrorKind.NotFound => controller.NotFound(body),
                ErrorKind.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, body),
                _ => controller.BadRequest(body)
            };
        }
    }
}
