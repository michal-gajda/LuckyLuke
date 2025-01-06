using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Refit;
using RinTinTin;
using RinTinTin.Interfaces;

const string SERVICE_NAME = "RinTinTin";

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton(builder.Configuration.GetSection("RanTanPlan").Get<RanTanPlanOptions>()!);

builder.Services.AddRefitClient<IRanTanPlanService>()
    .ConfigureHttpClient((serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<RanTanPlanOptions>();
        client.BaseAddress = options.BaseAddress;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/weatherforecast", async ([FromServices] IRanTanPlanService ranTanPlanService, CancellationToken cancellationToken = default) =>
    await ranTanPlanService.GetWeatherForecasts(cancellationToken)
    ).WithName("GetWeatherForecast");

await app.RunAsync();
