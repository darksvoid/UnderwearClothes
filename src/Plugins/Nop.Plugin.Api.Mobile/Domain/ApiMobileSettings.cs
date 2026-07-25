using Nop.Core.Configuration;

namespace Nop.Plugin.Api.Mobile.Domain;

public class ApiMobileSettings : ISettings
{
    public string SecretKey { get; set; }

    public string Issuer { get; set; } = "nopCommerce.MobileApi";

    public string Audience { get; set; } = "nopCommerce.MobileClient";

    public int AccessTokenExpirationMinutes { get; set; } = 60;
}
