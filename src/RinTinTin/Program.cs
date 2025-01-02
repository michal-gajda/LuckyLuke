using Microsoft.AspNetCore.Mvc;
using Refit;
using RinTinTin;
using RinTinTin.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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
