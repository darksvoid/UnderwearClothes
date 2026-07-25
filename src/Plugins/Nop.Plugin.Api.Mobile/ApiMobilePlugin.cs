using System.Security.Cryptography;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Plugins;

namespace Nop.Plugin.Api.Mobile;

/// <summary>
/// Represents the Mobile REST API plugin
/// </summary>
public class ApiMobilePlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    protected readonly IWebHelper _webHelper;
    protected readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public ApiMobilePlugin(IWebHelper webHelper,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _settingService = settingService;
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
        //generate a strong random secret for signing JWT access tokens
        await _settingService.SaveSettingAsync(new ApiMobileSettings
        {
            SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<ApiMobileSettings>();

        await base.UninstallAsync();
    }

    #endregion
}
