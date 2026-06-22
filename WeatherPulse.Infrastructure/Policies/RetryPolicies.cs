using Polly;
using Polly.Extensions.Http;

namespace WeatherPulse.Infrastructure.Policies;

public static class RetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetExponentialRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408 timeouts
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429
            .WaitAndRetryAsync(
                3, // Retry 3 times (Matches SRS FR-F9)
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // 2s, 4s, 8s
            );
    }
}