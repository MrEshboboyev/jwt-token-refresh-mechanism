using Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace App.OptionsSetup;

public class TokenPolicyOptionsSetup(
    IConfiguration configuration
) : IConfigureOptions<TokenPolicyOptions>
{
    private const string SectionName = "TokenPolicy";

    public void Configure(TokenPolicyOptions options)
    {
        configuration.GetSection(SectionName).Bind(options);
    }
}
