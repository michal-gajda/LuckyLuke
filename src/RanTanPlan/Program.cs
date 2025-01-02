using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string OTLP_LOGS = "http://localhost:5341/ingest/otlp/v1/logs";
const string OTLP_TRACES = "http://localhost:5341/ingest/otlp/v1/traces";
const string SERVICE_NAME = "RanTanPlan";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(SERVICE_NAME)).AddOtlpExporter(configure => { configure.Endpoint = new Uri(OTLP_LOGS); configure.Protocol = OtlpExportProtocol.HttpProtobuf; });
});

builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(SERVICE_NAME))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddOtlpExporter(configure => { configure.Endpoint = new Uri(OTLP_TRACES); configure.Protocol = OtlpExportProtocol.HttpProtobuf; }))
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddOtlpExporter());

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
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
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
