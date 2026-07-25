namespace Nop.Plugin.Api.Mobile.Models.Auth;

/// <summary>
/// Represents the issued access token
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Gets or sets the JWT access token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the token type (always "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the token lifetime, in seconds
    /// </summary>
    public int ExpiresIn { get; set; }
}
