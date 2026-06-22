namespace WeatherPulse.Application.DTOs;

public class WeatherResponseDto
{
    public string City { get; set; }
    public double TempC { get; set; }
    public int Aqi { get; set; }
    public double Uv { get; set; }
    public int OutdoorScore { get; set; }
    public string Advice { get; set; } // "Great day for a run!"
}