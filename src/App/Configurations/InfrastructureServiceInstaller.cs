using Application.Abstractions.Logging;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Infrastructure;
using Infrastructure.Logging;
using Infrastructure.Security;
using Scrutor;

namespace App.Configurations;

public class InfrastructureServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services
            .Scan(
                selector => selector
                    .FromAssemblies(
                        AssemblyReference.Assembly,
                        Persistence.AssemblyReference.Assembly)
                    .AddClasses(false)
                    .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                    .AsMatchingInterface()
                    .WithScopedLifetime());
                    
        // Register HTTP context accessor for client info service
        services.AddHttpContextAccessor();
        
        // Register client info service
        services.AddScoped<IClientInfoService, ClientInfoService>();
        
        // Register token hasher service
        services.AddScoped<ITokenHasher, TokenHasher>();
        
        // Register distributed cache (using in-memory for simplicity, but can be replaced with Redis)
        services.AddDistributedMemoryCache();
        
        // Register refresh token blacklist service
        services.AddScoped<IRefreshTokenBlacklistService, RefreshTokenBlacklistService>();
        
        // Register concurrent login service
        services.AddScoped<IConcurrentLoginService, ConcurrentLoginService>();
        
        // Register token logger
        services.AddScoped<ITokenLogger, TokenLogger>();
    }
}
