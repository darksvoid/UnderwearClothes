using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

/// <summary>
/// Enables attribute routing for the Mobile API controllers.
/// nopCommerce does not call <c>MapControllers()</c> anywhere (it uses only convention-based
/// routing via <see cref="IRouteProvider"/>), so the API controllers would be unreachable
/// without this registration.
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //map all attribute-routed controllers ([ApiController] + [Route("api/v1/...")])
        endpointRouteBuilder.MapControllers();
    }

    /// <summary>
    /// Gets a priority of route provider. Registered before the storefront catch-all route.
    /// </summary>
    public int Priority => 100;
}
