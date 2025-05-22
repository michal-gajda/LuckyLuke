using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Refit;
using RinTinTin;
using RinTinTin.Interfaces;

const string SERVICE_NAME = "rin-tin-tin";
const string SERVICE_NAMESPACE = "lucky-luke";
const string SERVICE_VERSION = "1.0.0";
const string INSTANCE_ID = "development";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(_ => { });

builder.Services.AddHealthChecks();

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(SERVICE_NAME, SERVICE_NAMESPACE, SERVICE_VERSION, autoGenerateServiceInstanceId: false, INSTANCE_ID);

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.AddOtlpExporter();
});

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .SetResourceBuilder(resourceBuilder)
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter());

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(SERVICE_NAME, SERVICE_VERSION));
builder.Services.AddHttpContextAccessor();

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

app.UseHttpLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/weatherforecast", async ([FromServices] IRanTanPlanService ranTanPlanService, CancellationToken cancellationToken = default) =>
    await ranTanPlanService.GetWeatherForecasts(cancellationToken)
).WithName("GetWeatherForecast");

await app.RunAsync();
