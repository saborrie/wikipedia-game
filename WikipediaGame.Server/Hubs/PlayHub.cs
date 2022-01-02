using Microsoft.AspNetCore.SignalR;

using Orleans;

using WikipediaGame.Server.Grains;

namespace WikipediaGame.Server.Hubs
{
    public class PlayHub : Hub
    {
        private readonly IClusterClient orleans;

        public PlayHub(IClusterClient orleans)
        {
            this.orleans = orleans;
        }

        public override Task OnConnectedAsync()
        {
            this.GetGrain();
            return Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await this.GetGrain().DisconnectAsync();
        }

        public async Task CreateGame(string username) => await this.GetGrain().CreateGame(username);

        public async Task JoinGame(string username, string gameCode) => await this.GetGrain().JoinGameAsync(username, gameCode);

        public async Task LeaveGame() => await this.GetGrain().LeaveGameAsync();

        private Connection.IConnectionGrain GetGrain()
        {
            return this.orleans.GetGrain<Connection.IConnectionGrain>(this.Context.ConnectionId);
        }
    }
}
