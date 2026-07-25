using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Api.Mobile.Models.Auth;

/// <summary>
/// Represents the login request body
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the customer username or email (depending on store configuration)
    /// </summary>
    [Required]
    public string UsernameOrEmail { get; set; }

    /// <summary>
    /// Gets or sets the customer password
    /// </summary>
    [Required]
    public string Password { get; set; }
}
