using WeatherPulse.Application.DTOs;

namespace WeatherPulse.Application.Interfaces;

public interface IWeatherClient
{
    Task<WeatherResponseDto> GetWeatherAsync(string city, CancellationToken ct);
}