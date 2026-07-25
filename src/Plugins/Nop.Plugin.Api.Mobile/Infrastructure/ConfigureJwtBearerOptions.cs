using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nop.Plugin.Api.Mobile.Domain;
using Nop.Plugin.Api.Mobile.Services.Security;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    #region Fields

    protected readonly IServiceProvider _serviceProvider;

    #endregion

    #region Ctor

    public ConfigureJwtBearerOptions(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #endregion

    #region Methods

    public void Configure(string name, JwtBearerOptions options)
    {
        if (name != ApiMobileDefaults.AuthenticationScheme)
            return;

        using var scope = _serviceProvider.CreateScope();
        var settings = scope.ServiceProvider.GetRequiredService<ApiMobileSettings>();

        var secret = string.IsNullOrEmpty(settings.SecretKey)
            ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            : settings.SecretKey;

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
                var blacklistService = context.HttpContext.RequestServices.GetRequiredService<IBlacklistService>();

                if (await blacklistService.IsBlacklistedAsync(jti))
                    context.Fail("This token has been revoked.");
            }
        };
    }

    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }

    #endregion
}
