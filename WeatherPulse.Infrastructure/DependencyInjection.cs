using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using WeatherPulse.Application.Interfaces;
using WeatherPulse.Infrastructure.Clients;
using WeatherPulse.Infrastructure.Policies;

namespace WeatherPulse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Settings
        services.Configure<OpenWeatherSettings>(configuration.GetSection("OpenWeather"));

        // Register HttpClient with Polly Retry Policy
        services.AddHttpClient<IWeatherClient, OpenWeatherClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        }).AddPolicyHandler(RetryPolicies.GetExponentialRetryPolicy()); // Applies 2s, 4s, 8s retries

        // Redis Distributed Cache (matches SRS Redis TTL)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "WeatherPulse_";
        });

        // Hangfire (for background jobs)
        services.AddHangfire(config => config
            .UsePostgreSqlStorage(configuration.GetConnectionString("HangfireDB")));
        services.AddHangfireServer();

        return services;
    }
}