using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string SERVICE_NAME = "RanTanPlan";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(_ => { });

builder.Services.AddHealthChecks();

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(SERVICE_NAME)).AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(SERVICE_NAME))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddOtlpExporter())
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddOtlpExporter());

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(SERVICE_NAME));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHealthChecks("/health");

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Balmy",
    "Bracing",
    "Chilly",
    "Cool",
    "Freezing",
    "Hot",
    "Mild",
    "Scorching",
    "Sweltering",
    "Warm",
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
}
).WithName("GetWeatherForecast");

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
