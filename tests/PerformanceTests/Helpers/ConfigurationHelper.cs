namespace PerformanceTests.Helpers;

public static class ConfigurationHelper
{
    public static string ApiBaseUrl => 
        Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080";
        
    public static int ConcurrentUsers => 
        int.TryParse(Environment.GetEnvironmentVariable("CONCURRENT_USERS"), out var users) ? users : 10;
        
    public static int TestDurationMinutes => 
        int.TryParse(Environment.GetEnvironmentVariable("TEST_DURATION_MINUTES"), out var minutes) ? minutes : 1;
}
