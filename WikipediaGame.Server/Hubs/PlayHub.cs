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

        public async Task CreateGame(string username) => await this.GetGrain().CreateGameAsync(username);
        public async Task JoinGame(string username, string gameCode) => await this.GetGrain().JoinGameAsync(username, gameCode);
        public async Task LeaveGame() => await this.GetGrain().LeaveGameAsync();
        public async Task BecomeGuesser() => await this.GetGrain().BecomeGuesserAsync();
        public async Task Reset() => await this.GetGrain().ResetAsync();
        public async Task StartRound() => await this.GetGrain().StartRoundAsync();
        public async Task MakeGuess(string username) => await this.GetGrain().MakeGuessAsync(username);

        public async Task SetArticle(string id, string name, string description, string extract) => await this.GetGrain().SetArticleAsync(new Game.Article
        {
            Id = id,
            Name = name,
            Description = description,
            Extract = extract
        });

        public async Task RemoveArticle() => await this.GetGrain().SetArticleAsync(null);

        private Connection.IConnectionGrain GetGrain()
        {
            return this.orleans.GetGrain<Connection.IConnectionGrain>(this.Context.ConnectionId);
        }       
    }
}
