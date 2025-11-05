using App.OptionsSetup;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace App.Configurations;

public class SwaggerServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        // Add NSwag services
        services.AddSwaggerDocument(config =>
        {
            config.PostProcess = (settings =>
            {
                settings.Info.Title = "JWT Token Refresh Mechanism API";
                settings.Info.Version = "v1";
                settings.Info.Description = "API for JWT token refresh mechanism with secure authentication";
            });
            
            // Add JWT bearer token authentication
            config.AddSecurity("JWT", [], new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}"
            });
            
            config.OperationProcessors.Add(new OperationSecurityScopeProcessor("JWT"));
        });
        
        // Configure ReDoc options
        services.ConfigureOptions<ReDocOptionsSetup>();
    }
}
