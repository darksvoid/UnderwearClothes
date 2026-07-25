using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Mobile.Factories;
using Nop.Plugin.Api.Mobile.Models;
using Nop.Plugin.Api.Mobile.Services.Security;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ApiExceptionFilter>();

        services.AddScoped<ICatalogModelFactory, CatalogModelFactory>();
        services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
        services.AddScoped<IOrderModelFactory, OrderModelFactory>();
        services.AddScoped<ICartModelFactory, CartModelFactory>();

        services.AddMemoryCache();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IBlacklistService, BlacklistService>();
        services.AddScoped<IApiAuthenticationService, ApiAuthenticationService>();
        services.AddScoped<SetWorkContextCustomerFilter>();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
        services.AddAuthentication().AddJwtBearer(ApiMobileDefaults.AuthenticationScheme, _ => { });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var details = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value.Errors.Select(error => error.ErrorMessage).ToArray());

                var body = ApiResponse.Fail("validation_error", "One or more validation errors occurred.", details);
                return new BadRequestObjectResult(body);
            };
        });

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(ApiMobileDefaults.SwaggerDocName, new OpenApiInfo
            {
                Title = ApiMobileDefaults.ApiTitle,
                Version = ApiMobileDefaults.SwaggerDocName,
                Description = "Private REST API for the mobile client application."
            });

            options.DocInclusionPredicate((_, apiDescription) =>
                apiDescription.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ?? false);

            var xmlPath = Path.Combine(
                Path.GetDirectoryName(typeof(NopStartup).Assembly.Location) ?? string.Empty,
                "Nop.Plugin.Api.Mobile.xml");
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter the JWT access token obtained from /api/v1/auth/token."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    public void Configure(IApplicationBuilder application)
    {
        application.UseSwagger();
        application.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                $"/{ApiMobileDefaults.SwaggerRoutePrefix}/{ApiMobileDefaults.SwaggerDocName}/swagger.json",
                $"{ApiMobileDefaults.ApiTitle} {ApiMobileDefaults.SwaggerDocName}");
            options.RoutePrefix = ApiMobileDefaults.SwaggerRoutePrefix;
            options.DocumentTitle = ApiMobileDefaults.ApiTitle;
        });
    }

    public int Order => 700;
}
