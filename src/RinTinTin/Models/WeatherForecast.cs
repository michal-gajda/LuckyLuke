namespace RinTinTin.Models;

internal sealed record class WeatherForecast
{
    public required DateOnly Date { get; init; }
    public required int TemperatureC { get; init; }
    public required int TemperatureF { get; init; }
    public required string? Summary { get; init; }
}
