using System.Security.Claims;
using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Api.Mobile.Services.Security;

/// <summary>
/// Handles Mobile API sign-in (issuing a token) and sign-out (revoking a token).
/// </summary>
public interface IApiAuthenticationService
{
    /// <summary>
    /// Validates customer credentials and, on success, issues an access token.
    /// </summary>
    Task<LoginResult> LoginAsync(string usernameOrEmail, string password);

    /// <summary>
    /// Revokes the current access token (adds it to the blacklist).
    /// </summary>
    Task LogoutAsync(ClaimsPrincipal userPrincipal);
}

/// <summary>
/// Outcome of a login attempt.
/// </summary>
public class LoginResult
{
    /// <summary>
    /// Gets or sets the validation status
    /// </summary>
    public CustomerLoginResults Status { get; set; }

    /// <summary>
    /// Gets or sets the issued access token (only when <see cref="Succeeded"/>)
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the access token lifetime, in seconds
    /// </summary>
    public int ExpiresInSeconds { get; set; }

    /// <summary>
    /// Gets a value indicating whether the login succeeded
    /// </summary>
    public bool Succeeded => Status == CustomerLoginResults.Successful && !string.IsNullOrEmpty(AccessToken);
}
