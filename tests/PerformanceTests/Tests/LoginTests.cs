using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Helpers;

namespace PerformanceTests.Tests;

public static class LoginTests
{
    public static ScenarioProps CreateScenario(HttpClient httpClient)
    {
        var scenario = Scenario.Create("user_login_scenario", async context =>
        {
            // First, we need to register a user to login with
            var user = TestDataHelper.GenerateRandomUser();
            
            // Register the user
            var registerRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/register")
                .WithJsonBody(user)
                .WithHeader("Accept", "application/json");
            
            var registerResponse = await Http.Send(httpClient, registerRequest);
            
            // Now login with the same credentials
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
