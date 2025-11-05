using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Helpers;
using PerformanceTests.Models;

namespace PerformanceTests.Tests;

public static class RefreshTokenTests
{
    public static ScenarioProps CreateScenario(HttpClient httpClient)
    {
        var scenario = Scenario.Create("refresh_token_scenario", async context =>
        {
            // First, we need to register and login a user
            var user = TestDataHelper.GenerateRandomUser();
            
            // Register the user
            var registerRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/register")
                .WithJsonBody(user)
                .WithHeader("Accept", "application/json");
            
            var registerResponse = await Http.Send(httpClient, registerRequest);
            
            // Login to get tokens
            var loginRequestData = TestDataHelper.GenerateLoginRequest(user.Email, user.Password);
            
            var loginRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/login")
                .WithJsonBody(loginRequestData)
                .WithHeader("Accept", "application/json");
            
            var loginResponse = await Http.Send(httpClient, loginRequest);
            
            // Get the refresh token from the login response
            var refreshTokenRequest = new RefreshTokenRequest("dummy_token");
            
            var request = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/refresh-token")
                .WithJsonBody(refreshTokenRequest)
                .WithHeader("Accept", "application/json");
            
            var response = await Http.Send(httpClient, request);
            
            return response;
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
