using Nop.Core.Configuration;

namespace Nop.Plugin.Api.Mobile.Domain;

/// <summary>
/// Represents the Mobile API plugin settings (persisted in the nopCommerce settings store).
/// </summary>
public class ApiMobileSettings : ISettings
{
    /// <summary>
    /// Gets or sets the secret key used to sign and validate JWT access tokens (HMAC-SHA256).
    /// Auto-generated on plugin installation.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the token issuer (iss claim)
    /// </summary>
    public string Issuer { get; set; } = "nopCommerce.MobileApi";

    /// <summary>
    /// Gets or sets the token audience (aud claim)
    /// </summary>
    public string Audience { get; set; } = "nopCommerce.MobileClient";

    /// <summary>
    /// Gets or sets the access token lifetime, in minutes
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;
}
