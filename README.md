# Lucky Luke

## OTEL

```csharp
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
```

```powershell
OTEL_EXPORTER_OTLP_METRICS_ENDPOINT=
```

### Logs/Tracing

```powershell
OTEL_EXPORTER_OTLP_TRACES_ENDPOINT
```

### Metrics

#### Prometeus

```powershell
.\prometheus.exe --web.enable-otlp-receiver
```

#### Grafana

##### Dashboard

[ASP.NET Core](https://grafana.com/grafana/dashboards/19924-asp-net-core/)
[ASP.NET Core Endpoint](https://grafana.com/grafana/dashboards/19925-asp-net-core-endpoint/)
