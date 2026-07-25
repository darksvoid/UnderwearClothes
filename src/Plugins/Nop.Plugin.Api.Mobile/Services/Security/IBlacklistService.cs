using System.Security.Claims;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public interface IBlacklistService
{
    Task BlacklistTokenAsync(ClaimsPrincipal userPrincipal, CancellationToken ct = default);

    Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default);
}
