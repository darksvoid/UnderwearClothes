namespace Nop.Plugin.Api.Mobile;

/// <summary>
/// Represents constants for the Mobile REST API plugin
/// </summary>
public static class ApiMobileDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public const string SystemName = "Api.Mobile";

    /// <summary>
    /// Gets the common route prefix for all API endpoints.
    /// Declared as a const so it can be used inside the [Route] attribute.
    /// </summary>
    public const string ApiRoutePrefix = "api/v1";

    /// <summary>
    /// Gets the Swagger document name (version)
    /// </summary>
    public const string SwaggerDocName = "v1";

    /// <summary>
    /// Gets the relative path where the Swagger UI is served
    /// </summary>
    public const string SwaggerRoutePrefix = "swagger";

    /// <summary>
    /// Gets the API title shown in the Swagger document
    /// </summary>
    public const string ApiTitle = "nopCommerce Mobile API";
}
