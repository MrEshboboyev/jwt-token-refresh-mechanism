using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Helpers;
using PerformanceTests.Models;

namespace PerformanceTests.Tests;

public static class UserJourneyTests
{
    public static ScenarioProps CreateScenario(HttpClient httpClient)
    {
        var scenario = Scenario.Create("complete_user_journey_scenario", async context =>
        {
            // Step 1: Register user
            var user = TestDataHelper.GenerateRandomUser();
            
            var registerRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/register")
                .WithJsonBody(user)
                .WithHeader("Accept", "application/json");
            
            var registerResponse = await Http.Send(httpClient, registerRequest);
            
            // Step 2: Login user
            var loginRequestData = TestDataHelper.GenerateLoginRequest(user.Email, user.Password);
            
            var loginRequest = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/login")
                .WithJsonBody(loginRequestData)
                .WithHeader("Accept", "application/json");
            
            var loginResponse = await Http.Send(httpClient, loginRequest);
            
            // Step 3: Refresh access token
            var refreshTokenRequest = new RefreshTokenRequest("dummy_token");
            
            var refreshTokenRequestMsg = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/refresh-token")
                .WithJsonBody(refreshTokenRequest)
                .WithHeader("Accept", "application/json");
            
            var refreshTokenResponse = await Http.Send(httpClient, refreshTokenRequestMsg);
            
            // Step 4: Revoke refresh token
            var revokeTokenRequest = new RevokeTokenRequest("dummy_token");
            
            var revokeTokenRequestMsg = Http.CreateRequest("POST", $"{ConfigurationHelper.ApiBaseUrl}/api/users/revoke-token")
                .WithJsonBody(revokeTokenRequest)
                .WithHeader("Accept", "application/json");
            
            var revokeTokenResponse = await Http.Send(httpClient, revokeTokenRequestMsg);
            
            // Return the last response
            return revokeTokenResponse;
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
