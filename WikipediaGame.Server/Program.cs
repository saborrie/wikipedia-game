using Microsoft.AspNetCore.ResponseCompression;

using Serilog;

using WikipediaGame.Server.Hubs;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// makes signalR use compression.
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services.AddSignalR();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapHub<PlayHub>("/hubs/play");
app.MapFallbackToFile("index.html");
app.Run();
    