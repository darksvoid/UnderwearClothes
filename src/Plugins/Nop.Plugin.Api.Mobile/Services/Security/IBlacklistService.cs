using System.Security.Claims;

namespace Nop.Plugin.Api.Mobile.Services.Security;

/// <summary>
/// Maintains a list of revoked JWT access tokens (by their jti claim).
/// In-memory implementation: entries live only for the remaining lifetime of the token
/// and are cleared on application restart.
/// </summary>
public interface IBlacklistService
{
    /// <summary>
    /// Revokes the token represented by the given principal until its natural expiration.
    /// </summary>
    Task BlacklistTokenAsync(ClaimsPrincipal userPrincipal, CancellationToken ct = default);

    /// <summary>
    /// Determines whether a token with the given jti has been revoked.
    /// </summary>
    Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default);
}
