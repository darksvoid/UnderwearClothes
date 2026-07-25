using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Mobile.Infrastructure;
using Nop.Plugin.Api.Mobile.Models;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Base controller for all Mobile API endpoints. Applies the common route prefix,
/// JSON content type, API behavior and the uniform exception handling.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route(ApiMobileDefaults.ApiRoutePrefix + "/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ServiceFilter(typeof(SetWorkContextCustomerFilter))]
[ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Wraps a payload into the success envelope and returns 200 OK
    /// </summary>
    protected IActionResult Success<T>(T data)
    {
        return Ok(ApiResponse<T>.Ok(data));
    }
}
