using WeatherPulse.Application.DTOs;
using WeatherPulse.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace WeatherPulse.Infrastructure.Clients;

public class OpenWeatherClient : IWeatherClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenWeatherClient(HttpClient httpClient, IOptions<OpenWeatherSettings> settings)
    {
        _httpClient = httpClient;
        _apiKey = settings.Value.ApiKey;
        _httpClient.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/");
    }

    public async Task<WeatherResponseDto> GetWeatherAsync(string city, CancellationToken ct)
    {
        // 1. Geocode first (to get lat/lon)
        var geoUrl = $"https://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={_apiKey}";
        var geoResponse = await _httpClient.GetAsync(geoUrl, ct);
        var geoJson = await geoResponse.Content.ReadAsStringAsync(ct);
        using var geoDoc = JsonDocument.Parse(geoJson);
        var lat = geoDoc.RootElement[0].GetProperty("lat").GetDouble();
        var lon = geoDoc.RootElement[0].GetProperty("lon").GetDouble();

        // 2. Fetch Weather, AQI, and UV in PARALLEL (Task.WhenAll)
        var weatherTask = _httpClient.GetAsync($"weather?lat={lat}&lon={lon}&units=metric&appid={_apiKey}", ct);
        var aqiTask = _httpClient.GetAsync($"air_pollution?lat={lat}&lon={lon}&appid={_apiKey}", ct);
        var uvTask = _httpClient.GetAsync($"uvi?lat={lat}&lon={lon}&appid={_apiKey}", ct);

        await Task.WhenAll(weatherTask, aqiTask, uvTask); // << THE MAGIC LINE

        // 3. Parse Results
        var weatherJson = await weatherTask.Result.Content.ReadAsStringAsync(ct);
        var aqiJson = await aqiTask.Result.Content.ReadAsStringAsync(ct);
        var uvJson = await uvTask.Result.Content.ReadAsStringAsync(ct);

        var temp = JsonDocument.Parse(weatherJson).RootElement.GetProperty("main").GetProperty("temp").GetDouble();
        var aqi = JsonDocument.Parse(aqiJson).RootElement.GetProperty("list")[0].GetProperty("main").GetProperty("aqi").GetInt32();
        var uv = JsonDocument.Parse(uvJson).RootElement.GetProperty("value").GetDouble();

        // 4. Calculate Score (Simple logic)
        var score = 0;
        if (temp >= 15 && temp <= 25) score += 40;
        else if (temp > 25 && temp < 35) score += 20;

        if (aqi <= 50) score += 30; // Good AQI
        else if (aqi <= 100) score += 10;

        if (uv < 3) score += 30;
        else if (uv < 6) score += 10;

        var advice = score >= 80 ? "Perfect for running!" : score >= 50 ? "Moderate, proceed with caution." : "Stay indoors.";

        return new WeatherResponseDto
        {
            City = city,
            TempC = temp,
            Aqi = aqi,
            Uv = uv,
            OutdoorScore = score,
            Advice = advice
        };
    }
}

public class OpenWeatherSettings
{
    public string ApiKey { get; set; }
}