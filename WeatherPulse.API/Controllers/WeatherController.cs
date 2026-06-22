using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WeatherPulse.Application.DTOs;
using WeatherPulse.Application.Interfaces;

namespace WeatherPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherClient _weatherClient;
    private readonly IDistributedCache _cache;

    public WeatherController(IWeatherClient weatherClient, IDistributedCache cache)
    {
        _weatherClient = weatherClient;
        _cache = cache;
    }

    [HttpGet("score")]
    public async Task<ActionResult<WeatherResponseDto>> GetOutdoorScore([FromQuery] string city, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city))
            return BadRequest("City is required.");

        // CHECK CACHE FIRST!
        var cacheKey = $"outdoor:{city.ToLower()}";
        var cachedJson = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            var cachedResult = JsonSerializer.Deserialize<WeatherResponseDto>(cachedJson);
            return Ok(cachedResult);
        }

        // Cache MISS: Fetch from external APIs
        var result = await _weatherClient.GetWeatherAsync(city, ct);

        // STORE IN CACHE with 30-minute TTL
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), options, ct);

        return Ok(result);
    }
}