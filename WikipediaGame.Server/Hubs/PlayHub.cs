using Microsoft.AspNetCore.SignalR;

namespace WikipediaGame.Server.Hubs
{
    public class PlayHub : Hub
    {
        public async Task SayHello(string greeting)
        {
            await Clients.All.SendAsync("Update", greeting);
        }
    }
}
