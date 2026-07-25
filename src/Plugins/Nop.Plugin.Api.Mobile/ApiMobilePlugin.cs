using System.Security.Cryptography;
using Nop.Core;
using Nop.Plugin.Api.Mobile.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Plugins;

namespace Nop.Plugin.Api.Mobile;

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

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}{ApiMobileDefaults.SwaggerRoutePrefix}";
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new ApiMobileSettings
        {
            SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
        });

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<ApiMobileSettings>();

        await base.UninstallAsync();
    }

    #endregion
}
