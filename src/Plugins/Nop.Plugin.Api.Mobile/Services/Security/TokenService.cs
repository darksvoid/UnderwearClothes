using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Mobile.Domain;

namespace Nop.Plugin.Api.Mobile.Services.Security;

public class TokenService : ITokenService
{
    #region Fields

    protected readonly ApiMobileSettings _settings;
    protected readonly TimeProvider _timeProvider;
    protected readonly JwtSecurityTokenHandler _tokenHandler = new();

    #endregion

    #region Ctor

    public TokenService(ApiMobileSettings settings, TimeProvider timeProvider)
    {
        _settings = settings;
        _timeProvider = timeProvider;
    }

    #endregion

    #region Methods

    public string GenerateAccessToken(Customer customer)
    {
        if (string.IsNullOrEmpty(_settings.SecretKey))
            throw new InvalidOperationException("The Mobile API is not configured (missing signing key).");

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("customer_guid", customer.CustomerGuid.ToString())
        };

        if (!string.IsNullOrEmpty(customer.Email))
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, customer.Email));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    public int AccessTokenExpirationSeconds => _settings.AccessTokenExpirationMinutes * 60;

    #endregion
}
