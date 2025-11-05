using NBomber.CSharp;
using NBomber.Http.CSharp;
using PerformanceTests.Tests;

// Create HTTP client for all scenarios
var httpClient = Http.CreateDefaultClient();

// Define scenarios
var simpleScenario = SimpleApiTests.CreateScenario(httpClient);
var fullFlowScenario = FullJwtFlowTests.CreateScenario(httpClient);

// Run all scenarios
NBomberRunner
    .RegisterScenarios(simpleScenario, fullFlowScenario)
    .WithTestName("JWT Token Refresh Mechanism Performance Tests")
    .Run();
