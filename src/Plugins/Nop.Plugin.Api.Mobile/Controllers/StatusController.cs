using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Mobile.Models;

namespace Nop.Plugin.Api.Mobile.Controllers;

/// <summary>
/// Service endpoints used to verify that the Mobile API is up and reachable.
/// </summary>
public class StatusController : BaseApiController
{
    /// <summary>
    /// Returns a lightweight health/status payload.
    /// </summary>
    /// <response code="200">The API is up and running.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<StatusModel>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var status = new StatusModel
        {
            Name = ApiMobileDefaults.ApiTitle,
            ApiVersion = ApiMobileDefaults.SwaggerDocName,
            ServerTimeUtc = DateTime.UtcNow
        };

        return Success(status);
    }
}

/// <summary>
/// Represents the API status payload
/// </summary>
public class StatusModel
{
    /// <summary>
    /// Gets or sets the API name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the API version
    /// </summary>
    public string ApiVersion { get; set; }

    /// <summary>
    /// Gets or sets the current server time (UTC)
    /// </summary>
    public DateTime ServerTimeUtc { get; set; }
}
