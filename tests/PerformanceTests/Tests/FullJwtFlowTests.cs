using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Helpers;

namespace PerformanceTests.Tests;

public static class FullJwtFlowTests
{
    public static ScenarioProps CreateScenario(HttpClient httpClient)
    {
        var scenario = Scenario.Create("full_jwt_flow_scenario", async context =>
        {
            // Step 1: Register a new user
            var user = TestDataHelper.GenerateRandomUser();
            
            var registerRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/register")
                .WithJsonBody(user)
                .WithHeader("Accept", "application/json");
            
            var registerResponse = await Http.Send(httpClient, registerRequest);
            
            // Step 2: Login with the registered user
            var loginRequestData = TestDataHelper.GenerateLoginRequest(user.Email, user.Password);
            
            var loginRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/login")
                .WithJsonBody(loginRequestData)
                .WithHeader("Accept", "application/json");
            
            var loginResponse = await Http.Send(httpClient, loginRequest);
            
            // For now, just return the login response
            // In a more advanced version, we could extract tokens and test the full flow
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
