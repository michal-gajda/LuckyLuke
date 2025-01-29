using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Refit;
using RinTinTin;
using RinTinTin.Interfaces;

const string SERVICE_NAME = "RinTinTin";
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
          .AddHttpClientInstrumentation()
          .AddOtlpExporter())
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddProcessInstrumentation()
          .AddOtlpExporter(cfg =>
          {
              cfg.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
              cfg.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
          }));

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(SERVICE_NAME));

builder.Services.AddSingleton(builder.Configuration.GetSection("RanTanPlan").Get<RanTanPlanOptions>()!);

builder.Services.AddRefitClient<IRanTanPlanService>()
    .ConfigureHttpClient((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<RanTanPlanOptions>();
        client.BaseAddress = options.BaseAddress;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/weatherforecast", async ([FromServices] IRanTanPlanService ranTanPlanService, CancellationToken cancellationToken = default) =>
    await ranTanPlanService.GetWeatherForecasts(cancellationToken)
    ).WithName("GetWeatherForecast");

await app.RunAsync();
