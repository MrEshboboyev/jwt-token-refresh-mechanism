# JWT Token Refresh Mechanism Performance Tests

This project contains performance tests for the JWT Token Refresh Mechanism API using NBomber.

## Test Scenarios

1. **User Registration** - Tests the user registration endpoint
2. **User Login** - Tests the user login endpoint
3. **Token Refresh** - Tests the refresh token endpoint
4. **Token Revocation** - Tests the revoke token endpoint
5. **Complete User Journey** - Tests a complete flow from registration to token revocation
6. **Concurrent Logins** - Tests multiple concurrent logins for the same user

## Prerequisites

- .NET 9 SDK
- The JWT Token Refresh Mechanism API running locally or deployed

## Configuration

The tests can be configured using environment variables:

- `API_BASE_URL` - The base URL of the API (default: http://localhost:5000)
- `CONCURRENT_USERS` - Number of concurrent users to simulate (default: 10)
- `TEST_DURATION_MINUTES` - Duration of the test in minutes (default: 1)

## Running the Tests

1. Ensure the API is running
2. Navigate to the PerformanceTests directory
3. Run the tests:

```bash
dotnet run
```

## Running with Custom Configuration

```bash
API_BASE_URL=https://your-api-url.com CONCURRENT_USERS=50 TEST_DURATION_MINUTES=5 dotnet run
```

## Test Output

The tests will generate detailed performance reports including:

- Request rates (RPS)
- Response times (mean, max, percentiles)
- Success rates
- Data transfer statistics
