using WikipediaGame.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapHub<PlayHub>("/hubs/play");
app.MapFallbackToFile("index.html");
app.Run();
    