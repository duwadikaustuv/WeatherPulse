namespace WeatherPulse.Domain.Entities;

public class OutdoorScore
{
    public string City { get; set; }
    public double TemperatureCelsius { get; set; }
    public int Aqi { get; set; } // Air Quality Index
    public double UvIndex { get; set; }
    public int Score { get; set; } // 0 to 100
    public string Summary { get; set; } // "Perfect", "Moderate", "Unsafe"
    public DateTime FetchedAt { get; set; }
}