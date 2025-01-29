using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

const string SERVICE_NAME = "RanTanPlan";
const string INSTANCE_ID = "Dev";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(SERVICE_NAME, autoGenerateServiceInstanceId: false, serviceInstanceId: INSTANCE_ID)).AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(SERVICE_NAME, autoGenerateServiceInstanceId: false, serviceInstanceId: INSTANCE_ID))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddOtlpExporter())
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddRuntimeInstrumentation()
          .AddProcessInstrumentation()
          .AddOtlpExporter(cfg =>
          {
              cfg.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
              cfg.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
          }));

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(SERVICE_NAME));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHealthChecks("/health");

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
