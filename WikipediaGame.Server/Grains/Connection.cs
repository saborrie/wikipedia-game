using Microsoft.AspNetCore.SignalR;

using Orleans;

using WikipediaGame.Server.Hubs;

namespace WikipediaGame.Server.Grains
{
    public static class Connection
    {
        public interface IConnectionGrain : IGrainWithStringKey
        {
            Task CreateGameAsync(string username);
            Task JoinGameAsync(string username, string gameCode);
            Task LeaveGameAsync();
            Task DisconnectAsync();
            Task BecomeGuesserAsync();
            Task ResetAsync();
            Task SetArticleAsync(Game.Article? article);
            Task MakeGuessAsync(string username);
            Task StartRoundAsync();
            Task RequestUpdatedStateAsync();
            Task AckPing();
        }

        public class ConnectionGrain : Grain, IConnectionGrain, Game.IGameObserver
        {
            private readonly ILogger<ConnectionGrain> logger;
            private readonly PlayHubHelper hub;
            private string? gameCode;
            private string? username;
            private DateTime? lastDisconnected;
            private IDisposable? checkDisconnectionTimer;
            private IDisposable pingTimer;
            private PlayHubHelper.ConnectionUpdate lastState;
            private readonly static TimeSpan diconnectionTimeSpan = TimeSpan.FromMinutes(1);

            public string ConnectionId => this.GetPrimaryKeyString();

            public ConnectionGrain(ILogger<ConnectionGrain> logger, PlayHubHelper hub)
            {
                this.logger = logger;
                this.hub = hub;
            }

            public override async Task OnActivateAsync()
            {
                this.ResetState();
                this.SendUpdatedState();
                
                this.checkDisconnectionTimer = this.RegisterTimer(this.CheckDisconnection, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

                this.pingTimer = this.RegisterTimer(this.PingAsync, null, TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));

                await base.OnActivateAsync();
            }

            private async Task CheckDisconnection(object arg)
            {
                if (this.lastDisconnected.HasValue && (this.lastDisconnected.Value + diconnectionTimeSpan) < DateTime.UtcNow)
                {
                    this.checkDisconnectionTimer?.Dispose();
                    this.pingTimer?.Dispose();
                    await this.LeaveGameAsync();
                    this.DeactivateOnIdle();
                }
            }

            public async Task CreateGameAsync(string username)
            {
                if (this.lastState.InGame)
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
                if (!this.lastState.InGame)
                {
                    throw new InvalidOperationException("Cannot leave game. Not in a game.");
                }

                if (this.username is null)
                {
                    throw new InvalidOperationException("Cannot leave game. No username.");
                }

                var game = this.GetGame();
                await game.Unsubscribe(this);
                await game.RemovePlayerAsync(this.username);
                this.username = null;
                this.gameCode = null;
                ResetState();
                SendUpdatedState();
            }

            private void ResetState()
            {
                this.lastState = new PlayHubHelper.ConnectionUpdate
                {
                    InGame = false,
                };
            }

            public void OnGameUpdated(Game.GameState state)
            {
                this.lastState = new PlayHubHelper.ConnectionUpdate
                {
                    InGame = true,
                    Game = new PlayHubHelper.GameView
                    {
                        GameCode = state.GameCode,
                        Events = state.Events,
                        Answer = state.Answer != null ? new PlayHubHelper.AnswerView
                        {
                            Username = state.Answer.Username,
                            Article = MapToArticle(state.Answer.Article)
                        } : null,
                        InPlay = state.InPlay,
                        Revealed = state.Revealed,
                        Clue = state.Clue,
                        Username = this.username,
                        Options = state.Options,
                        Players = state.Players?.Select(x => new PlayHubHelper.PlayerView
                        {
                            Name = x.Name,
                            IsGuesser = x.IsGuesser,
                            HasArticle = x.HasArticle,
                        }).ToList(),
                        Article = MapToArticle(state.Players?.FirstOrDefault(x => x.Name == this.username)?.Article)
                    }
                };
                SendUpdatedState();
            }

            private void SendUpdatedState()
            {
                _ = this.hub.UpdateAsync(this.ConnectionId, this.lastState);
            }

            private static PlayHubHelper.ArticleView? MapToArticle(Game.Article? article)
            {
                if (article == null) return null;

                return new PlayHubHelper.ArticleView
                {
                    Id = article.Id,
                    Name = article.Name,
                    Description = article.Description,
                    Extract = article.Extract,
                };
            }

            public async Task DisconnectAsync()
            {
                    this.lastDisconnected = DateTime.UtcNow;
            }

            public async Task BecomeGuesserAsync()
            {
                CheckInGame("become guesser");
                await GetGame().SetGuesserAsync(this.username!);
            }

            public async Task ResetAsync()
            {
                CheckInGame("reset");
                await GetGame().ResetAsync();
            }

            public async Task MakeGuessAsync(string username)
            {
                CheckInGame("make guess");
                await GetGame().MakeGuessAsync(this.username!, username);
            }

            public async Task SetArticleAsync(Game.Article? article)
            {
                CheckInGame("set article");
                await GetGame().SetArticleAsync(this.username!, article);
            }
            public async Task StartRoundAsync()
            {
                CheckInGame("start round");
                await GetGame().StartRoundAsync();
            }

            private void CheckInGame(string action)
            {
                if (this.gameCode is null)
                {
                    throw new InvalidOperationException($"Cannot {action}, not in a game.");
                }
            }

            public Task RequestUpdatedStateAsync()
            {
                this.lastDisconnected = null;
                this.SendUpdatedState();
                return Task.CompletedTask;
            }

            public Task AckPing()
            {
                // TODO figure out what to do when we get this back.
                this.logger.LogInformation("Ping acknowledged {connectionId}", this.ConnectionId);
                return Task.CompletedTask;
            }

            private async Task PingAsync(object arg)
            {
                await this.hub.PingAsync(this.ConnectionId);
                this.logger.LogInformation("Pinged {connectionId}.", this.ConnectionId);
            }

        }
    }
}
