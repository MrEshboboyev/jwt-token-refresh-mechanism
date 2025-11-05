using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Helpers;

namespace PerformanceTests.Tests;

public static class ConcurrentLoginTests
{
    public static ScenarioProps CreateScenario(HttpClient httpClient)
    {
        var scenario = Scenario.Create("concurrent_login_scenario", async context =>
        {
            // Step 1: Create a single user for concurrent login testing
            var user = TestDataHelper.GenerateRandomUser();
            
            // Register the user
            var registerRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/register")
                .WithJsonBody(user)
                .WithHeader("Accept", "application/json");
            
            var registerResponse = await Http.Send(httpClient, registerRequest);
            
            // Step 2: Login with the user
            var loginRequestData = TestDataHelper.GenerateLoginRequest(user.Email, user.Password);
            
            var loginRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/login")
                .WithJsonBody(loginRequestData)
                .WithHeader("Accept", "application/json");
            
            var loginResponse = await Http.Send(httpClient, loginRequest);
            
            return loginResponse;
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(
                rate: ConfigurationHelper.ConcurrentUsers,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromMinutes(ConfigurationHelper.TestDurationMinutes)
            )
        );
        
        return scenario;
    }
}
