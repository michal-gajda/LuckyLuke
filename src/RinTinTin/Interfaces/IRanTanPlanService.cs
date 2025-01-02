namespace RinTinTin.Interfaces;

using Refit;
using RinTinTin.Models;

internal interface IRanTanPlanService
{
    [Get("/weatherforecast")]
    Task<IEnumerable<WeatherForecast>> GetWeatherForecasts(CancellationToken cancellationToken = default);
}
