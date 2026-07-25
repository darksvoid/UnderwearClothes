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

/// <summary>
/// Configures the JWT bearer scheme lazily. The signing key and options are read from the
/// plugin settings (DB) the first time the scheme is materialized — i.e. after the data layer
/// is ready — which avoids touching the database during application service configuration.
/// </summary>
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

        //if the plugin is not configured yet, use a throwaway key so that no token can be validated
        var secret = string.IsNullOrEmpty(settings.SecretKey)
            ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            : settings.SecretKey;

        //keep the original JWT claim names (sub, jti, ...) instead of the ASP.NET mappings
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
            //reject tokens that have been revoked (present in the blacklist)
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
