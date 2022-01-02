using Microsoft.AspNetCore.SignalR;

using Orleans;

using WikipediaGame.Server.Hubs;

namespace WikipediaGame.Server.Grains
{
    public static class Connection
    {
        public interface IConnectionGrain : IGrainWithStringKey
        {
            Task CreateGame(string username);
            Task JoinGameAsync(string username, string gameCode);
            Task LeaveGameAsync();
            Task DisconnectAsync();
        }

        public class ConnectionGrain : Grain, IConnectionGrain, Game.IGameObserver
        {
            private readonly PlayHubHelper hub;
            private string? gameCode;
            private string? username;

            public string ConnectionId => this.GetPrimaryKeyString();

            public ConnectionGrain(PlayHubHelper hub)
            {
                this.hub = hub;
            }

            public async Task CreateGame(string username)
            {
                if (this.gameCode != null)
                {
                    throw new InvalidOperationException("Cannot create a new game, must leave current game first.");
                }
                
                this.gameCode = await this.GetGameManager().GetNextGameCodeAsync();

                await JoinGameAsync(username);
            }

            public async Task JoinGameAsync(string username, string gameCode)
            {
                if (this.gameCode != null)
                {
                    throw new InvalidOperationException("Cannot join a new game, must leave current game first.");
                }

                if (gameCode is null)
                {
                    throw new ArgumentNullException(nameof(gameCode));
                }

                if (!await this.GetGameManager().GameExistsAsync(gameCode))
                {
                    throw new InvalidOperationException($"Cannot join game {gameCode}, game not found.");
                }

                this.gameCode = gameCode;

                await JoinGameAsync(username);
            }
            private GameManager.IGameManagerGrain GetGameManager()
            {
                return this.GrainFactory.GetGrain<GameManager.IGameManagerGrain>(Guid.Empty);
            }

            private Game.IGameGrain GetGame()
            {
                return this.GrainFactory.GetGrain<Game.IGameGrain>(this.gameCode);
            }

            private async Task JoinGameAsync(string username)
            {
                var game = this.GetGame();
                await game.SubscribeAsync(this);
                await game.AddPlayerAsync(username);

                this.username = username ?? throw new ArgumentNullException(nameof(username));
            }

            public async Task LeaveGameAsync()
            {
                if (this.gameCode is null)
                {
                    throw new InvalidOperationException("Cannot leave game. Not in a game.");
                }

                if (this.username is null)
                {
                    throw new InvalidOperationException("Cannot leave game. No username.");
                }

                var game = this.GetGame();
                await game.RemovePlayerAsync(this.username);
                await game.Unsubscribe(this);
                this.username = null;
                this.gameCode = null;

            }

            public void OnGameUpdated(Game.GameState state)
            {
                _ = this.hub.UpdateAsync(this.ConnectionId, new PlayHubHelper.ConnectionUpdate
                {
                    InGame = true,
                    Game = new PlayHubHelper.GameView
                    {
                        GameCode = state.GameCode,
                        Players = state.Players?.Select(x => new PlayHubHelper.PlayerView
                        {
                            Name = x.Name,
                        }).ToList()
                    }
                });
            }

            public async Task DisconnectAsync()
            {
                await this.LeaveGameAsync();
                this.DeactivateOnIdle();
            }
        }
    }
}
