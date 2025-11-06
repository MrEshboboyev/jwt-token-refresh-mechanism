using App.Configurations;
using App.Middlewares;
using App.OptionsSetup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Install services from assemblies implementing IServiceInstaller
builder.Services
    .InstallServices(
        builder.Configuration,
        typeof(IServiceInstaller).Assembly);

// Configure Serilog for logging
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Use NSwag middleware for Swagger UI and ReDoc
    app.UseOpenApi();
    app.UseSwaggerUi();
    
    // Configure ReDoc with custom settings from appsettings.json
    var reDocOptions = app.Services.GetRequiredService<IOptions<ReDocOptions>>().Value;
    
    app.UseReDoc(config =>
    {
        config.Path = reDocOptions.Path;
        config.DocumentPath = reDocOptions.DocumentPath;
    });
}

app.UseHttpsRedirection();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Register the global exception handling middleware in the request processing pipeline
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Map controllers to route endpoints
app.MapControllers();

await AutoMigrateAsync(app);

app.Run();

// auto migrate method
static async Task AutoMigrateAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while migrating the database.");
        throw;
    }
}
