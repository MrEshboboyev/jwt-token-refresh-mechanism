using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Helpers;

namespace PerformanceTests.Tests;

public static class SimpleApiTests
{
    public static ScenarioProps CreateScenario(HttpClient httpClient)
    {
        var scenario = Scenario.Create("simple_api_test_scenario", async context =>
        {
            // Test the register endpoint
            var registerRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/register")
                .WithJsonBody(new { Email = "test@example.com", Password = "Password123!", FullName = "Test User" })
                .WithHeader("Accept", "application/json");
            
            var registerResponse = await Http.Send(httpClient, registerRequest);
            
            // Test the login endpoint
            var loginRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/login")
                .WithJsonBody(new { Email = "test@example.com", Password = "Password123!" })
                .WithHeader("Accept", "application/json");
            
            var loginResponse = await Http.Send(httpClient, loginRequest);
            
            // Return the last response
            return loginResponse;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(
                rate: 1,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(10)
            )
        );
        
        return scenario;
    }
}
