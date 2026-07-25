using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Plugins;

namespace Nop.Plugin.Api.Mobile;

/// <summary>
/// Represents the Mobile REST API plugin
/// </summary>
public class ApiMobilePlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public ApiMobilePlugin(IWebHelper webHelper)
    {
        _webHelper = webHelper;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL. Opens the Swagger UI of the API.
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}{ApiMobileDefaults.SwaggerRoutePrefix}";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await base.UninstallAsync();
    }

    #endregion
}
