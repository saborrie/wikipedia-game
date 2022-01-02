using Orleans;
using Orleans.Runtime;

using WikipediaGame.Server.Utilities;

namespace WikipediaGame.Server.Grains
{
    public static class Game
    {
        public interface IGameGrain : IGrainWithStringKey
        {
            Task AddPlayerAsync(string username);
            Task RemovePlayerAsync(string username);

            Task SubscribeAsync(IGameObserver observer);
            Task Unsubscribe(IGameObserver observer);
        }

        public class GameGrain : Grain, IGameGrain
        {
            private readonly ILogger<GameGrain> logger;
            private readonly ObserverManager<IGameObserver> observerManager;
            private readonly HashSet<string> playerIds;
            public GameGrain(ILogger<GameGrain> logger)
            {
                this.observerManager = new ObserverManager<IGameObserver>(TimeSpan.FromHours(6), logger, "subs");
                this.logger = logger;
                this.playerIds = new();
            }

            private string GameCode => this.GetPrimaryKeyString();

            public Task AddPlayerAsync(string username)
            {
                if (!this.playerIds.Add(username))
                {
                    throw new InvalidOperationException($"Cannot add player, player with username {username} already in game {this.GameCode}.");
                }

                this.logger.LogInformation("Player {Player} added to game {GameCode}", username, this.GameCode);
                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            public Task RemovePlayerAsync(string username)
            {
                if (!this.playerIds.Remove(username))
                {
                    throw new InvalidOperationException($"Cannot remove player, player with username {username} not found in game {this.GameCode}.");
                }

                this.logger.LogInformation("Player {Player} removed from game {GameCode}", username, this.GameCode);
                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            public Task SubscribeAsync(IGameObserver observer)
            {
                this.observerManager.Subscribe(observer, observer);
                return Task.CompletedTask;
            }

            public Task Unsubscribe(IGameObserver observer)
            {
                this.observerManager.Unsubscribe(observer);
                return Task.CompletedTask;
            }

            private void NotifySubscribers()
            {
                this.observerManager.Notify(x => x.OnGameUpdated(this.GetGameState()));
            }

            private GameState GetGameState()
            {
                return new GameState
                {
                    GameCode = this.GameCode,
                    Players = this.playerIds.Select(x => new Player
                    {
                        Name = x
                    }).ToList()
                };
            }
        }

        public interface IGameObserver : IGrainObserver
        {
            void OnGameUpdated(GameState state);
        }

        public record GameState
        {
            public IReadOnlyList<Player>? Players { get; init; }
            public string? GameCode { get; init; }
        }

        public record Player
        {
            public string? Name { get; init; }
        }
    }
}
