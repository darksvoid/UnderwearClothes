using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public class BlacklistService : IBlacklistService
{
    #region Fields

    protected readonly IMemoryCache _cache;
    protected readonly TimeProvider _timeProvider;

    #endregion

    #region Ctor

    public BlacklistService(IMemoryCache cache, TimeProvider timeProvider)
    {
        _cache = cache;
        _timeProvider = timeProvider;
    }

    #endregion

    #region Methods

    public Task BlacklistTokenAsync(ClaimsPrincipal userPrincipal, CancellationToken ct = default)
    {
        var jti = userPrincipal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var exp = userPrincipal.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(exp))
            return Task.CompletedTask;

        var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
        var remaining = expirationTime - _timeProvider.GetUtcNow();

        if (remaining > TimeSpan.Zero)
            _cache.Set(GetKey(jti), true, remaining);

        return Task.CompletedTask;
    }

    public Task<bool> IsBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(jti))
            return Task.FromResult(false);

        return Task.FromResult(_cache.TryGetValue(GetKey(jti), out _));
    }

    private static string GetKey(string jti)
    {
        return $"{ApiMobileDefaults.BlacklistCacheKeyPrefix}{jti}";
    }

    #endregion
}
