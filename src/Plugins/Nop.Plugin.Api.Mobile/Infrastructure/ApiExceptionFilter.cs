using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Nop.Plugin.Api.Mobile.Models;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

/// <summary>
/// Converts unhandled exceptions thrown by API controllers into the uniform
/// <see cref="ApiResponse{T}"/> error envelope with an appropriate status code.
/// </summary>
public class ApiExceptionFilter : IAsyncExceptionFilter
{
    #region Fields

    protected readonly ILogger<ApiExceptionFilter> _logger;

    #endregion

    #region Ctor

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Methods

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;

        var (statusCode, code) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "bad_request"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "not_found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "server_error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Mobile API unhandled exception");

        // Do not leak internal details for 500s
        var message = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;

        context.Result = new ObjectResult(ApiResponse.Fail(code, message))
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;

        return Task.CompletedTask;
    }

    #endregion
}
