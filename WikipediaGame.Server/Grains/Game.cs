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
            Task SetGuesserAsync(string username);
            Task SetArticleAsync(string username, Article? article);
            Task SubscribeAsync(IGameObserver observer);
            Task Unsubscribe(IGameObserver observer);
            Task MakeGuessAsync(string v, string username);
            Task StartRoundAsync();
            Task ResetAsync();
        }

        public class GameGrain : Grain, IGameGrain
        {
            private readonly ILogger<GameGrain> logger;
            private readonly ObserverManager<IGameObserver> observerManager;
            private readonly HashSet<string> playerIds;
            private string? guesser;
            private readonly Dictionary<string, Article> articles;
            private List<string> events;
            private List<string>? options;
            private Answer? answer;
            private bool inPlay;
            private bool revealed;

            public GameGrain(ILogger<GameGrain> logger)
            {
                this.observerManager = new ObserverManager<IGameObserver>(TimeSpan.FromHours(6), logger, "subs");
                this.logger = logger;
                this.playerIds = new();
                this.events = new();
                this.articles = new();
            }

            private string GameCode => this.GetPrimaryKeyString();

            public Task AddPlayerAsync(string username)
            {
                if (!this.playerIds.Add(username))
                {
                    throw new InvalidOperationException($"Cannot add player, player with username {username} already in game {this.GameCode}.");
                }

                this.logger.LogInformation("Player {Player} added to game {GameCode}", username, this.GameCode);
                this.events.Add($"{username} joined.");
                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            public Task RemovePlayerAsync(string username)
            {
                if (!this.playerIds.Remove(username))
                {
                    throw new InvalidOperationException($"Cannot remove player, player with username {username} not found in game {this.GameCode}.");
                }

                this.articles.Remove(username);
                if (this.guesser == username)
                {
                    this.guesser = null;
                }

                this.logger.LogInformation("Player {Player} removed from game {GameCode}", username, this.GameCode);
                this.events.Add($"{username} left.");
                this.NotifySubscribers();

                if (!this.playerIds.Any())
                {
                    this.DeactivateOnIdle();
                    this.GrainFactory.GetGrain<GameManager.IGameManagerGrain>(Guid.Empty).CloseGameAsync(this.GameCode);
                }

                return Task.CompletedTask;
            }

            public Task SetGuesserAsync(string username)
            {
                AssertNotInPlay("set guesser");

                if (!this.playerIds.Contains(username))
                {
                    throw new ArgumentException($"Cannot set guesser. Username {username} not found in game {this.GameCode}", nameof(username));
                }

                if (this.guesser == username)
                {
                    return Task.CompletedTask;
                }

                this.guesser = username;
                this.events.Add($"{username} is now the guesser.");
                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            private void AssertNotInPlay(string action)
            {
                if (this.inPlay)
                {
                    throw new ArgumentException($"Cannot {action} while game is in play.");
                }
            }

            public Task MakeGuessAsync(string guesser, string username)
            {
                if (!this.inPlay)
                {
                    throw new InvalidOperationException("Cannot make guess, not in-play.");
                }
                
                if (this.guesser != guesser)
                {
                    throw new ArgumentException($"Cannot make guess in game {this.GameCode}. Only the guesser can make guesses.", nameof(guesser));
                }

                if (!(this.options?.Contains(username) ?? false))
                {
                    throw new ArgumentException($"Cannot make guess. Username {username} is not an option in game {this.GameCode}", nameof(username));
                }

                this.events.Add($"{guesser} guessed {username}!");
                this.revealed = true;

                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            public Task ResetAsync()
            {
                if (!this.revealed)
                {
                    throw new InvalidOperationException("Cannot reset. Answer has not been revealed");
                }

                this.articles.Remove(answer!.Username!);

                this.revealed = false;
                this.answer = null;
                this.guesser = null;
                this.options = null;
                this.inPlay = false;

                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            public Task SetArticleAsync(string username, Article? article)
            {
                if (!this.playerIds.Contains(username))
                {
                    throw new ArgumentException($"Cannot set article. Username {username} not found in game {this.GameCode}", nameof(username));
                }

                if (article == null)
                {
                    this.articles.Remove(username);
                }
                else
                {
                    this.articles[username] = article;
                }

                this.NotifySubscribers();
                return Task.CompletedTask;
            }

            public Task StartRoundAsync()
            {
                AssertNotInPlay("start round");

                var potentialOptions = this.articles.Keys.Where(a => a != this.guesser).ToList();

                if (this.guesser == null || potentialOptions.Count < 2)
                {
                    throw new InvalidOperationException("Cannot start round. Must have a guesser and all articles ready.");
                }

                // randomly select a non-guesser
                var random = new Random();
                this.options = potentialOptions;
                int index = random.Next(options.Count);

                this.answer = new Answer
                {
                    Username = options[index],
                    Article = this.articles[options[index]]
                };

                this.inPlay = true;

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
                    Events = this.events,
                    Options = this.options,
                    InPlay = this.inPlay,
                    Revealed = this.revealed,
                    Clue = this.answer?.Article?.Name,
                    Answer = this.revealed ? this.answer : null,
                    Players = this.playerIds.Select(x => new Player
                    {
                        Name = x,
                        IsGuesser = x == this.guesser,
                        HasArticle = this.articles.ContainsKey(x),
                        Article = this.articles.GetValueOrDefault(x),
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
            public bool InPlay { get; init; }
            public bool Revealed { get; init; }
            public IReadOnlyList<string>? Options { get; init; }
            public IReadOnlyList<string>? Events { get; init; }
            public string? Clue { get; init; }
            internal Answer? Answer { get; init; }
        }

        public record Player
        {
            public string? Name { get; init; }
            public bool IsGuesser { get; init; }
            public bool HasArticle { get; init; }
            public Article? Article { get; init; }
        }

        public record Article
        {
            public string? Id { get; init; }
            public string? Name { get; init; }
            public string? Description { get; init; }
            public string? Extract { get; init; }
        }

        internal class Answer
        {
            public string? Username { get; init; }
            public Game.Article? Article { get; init; }
        }
    }
}
