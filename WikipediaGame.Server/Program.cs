using Microsoft.AspNetCore.ResponseCompression;

using Orleans.Hosting;
using Orleans;

using Serilog;

using WikipediaGame.Server.Hubs;
using static WikipediaGame.Server.Grains.Game;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Host.UseOrleans((context, siloBuilder) =>
{

    siloBuilder.ConfigureApplicationParts(parts =>
    {
        parts.AddApplicationPart(typeof(GameGrain).Assembly).WithReferences();
    });

    siloBuilder.ConfigureLogging(logging => logging.AddConsole());

    siloBuilder.UseLocalhostClustering(serviceId: "OrleansDemo", clusterId: "dev");
}).UseConsoleLifetime();

// makes signalR use compression.
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSingleton<PlayHubHelper>();
builder.Services.AddSignalR();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapHub<PlayHub>("/hubs/play");
app.MapFallbackToFile("index.html");
app.Run();
    