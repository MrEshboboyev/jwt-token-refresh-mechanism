using App.Configurations;
using Microsoft.Extensions.Options;

namespace App.OptionsSetup;

public class ReDocOptionsSetup(
    IConfiguration configuration
) : IConfigureOptions<ReDocOptions>
{
    private const string SectionName = "ReDoc";

    public void Configure(ReDocOptions options)
    {
        configuration.GetSection(SectionName).Bind(options);
    }
}

public class ReDocOptions
{
    public string Path { get; set; } = "/redoc";

    public string DocumentPath { get; set; } = "/swagger/v1/swagger.json";
}
