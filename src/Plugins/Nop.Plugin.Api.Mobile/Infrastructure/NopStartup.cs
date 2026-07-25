using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Mobile.Models;

namespace Nop.Plugin.Api.Mobile.Infrastructure;

/// <summary>
/// Registers the Mobile API services (Swagger, filters, API behavior) and middleware.
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the services
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //uniform exception handling for API controllers
        services.AddScoped<ApiExceptionFilter>();

        //return the uniform error envelope for model validation failures too
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

        //OpenAPI / Swagger generation (limited to our own /api/* endpoints)
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(ApiMobileDefaults.SwaggerDocName, new OpenApiInfo
            {
                Title = ApiMobileDefaults.ApiTitle,
                Version = ApiMobileDefaults.SwaggerDocName,
                Description = "Private REST API for the mobile client application."
            });

            //only document the mobile API, never the storefront/admin MVC controllers
            options.DocInclusionPredicate((_, apiDescription) =>
                apiDescription.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ?? false);

            //include XML comments when the generated doc file is present next to the plugin assembly
            var xmlPath = Path.Combine(
                Path.GetDirectoryName(typeof(NopStartup).Assembly.Location) ?? string.Empty,
                "Nop.Plugin.Api.Mobile.xml");
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
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

    /// <summary>
    /// Gets order of this startup configuration implementation.
    /// Placed after authorization (600) and before the endpoints (900) middleware.
    /// </summary>
    public int Order => 700;
}
