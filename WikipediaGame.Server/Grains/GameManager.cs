using HashidsNet;

using Orleans;

namespace WikipediaGame.Server.Grains
{
    public static class GameManager
    {
        public interface IGameManagerGrain : IGrainWithGuidKey
        {
            Task<string> GetNextGameCodeAsync();
            Task<bool> GameExistsAsync(string gameCode);
            Task CloseGameAsync(string gameCode);
        }

        public class GameManagerGrain : Grain, IGameManagerGrain
        {
            private int nextGameId = 0;
            private readonly Hashids hashids = new();

            private HashSet<string> activeGameCodes = new();

            public Task<string> GetNextGameCodeAsync()
            {
                var gameCode = this.hashids.Encode(nextGameId++);

                activeGameCodes.Add(gameCode);

                this.GrainFactory.GetGrain<Game.IGameGrain>(gameCode);

                return Task.FromResult(gameCode);
            }

            public Task<bool> GameExistsAsync(string gameCode)
            {
                return Task.FromResult(this.activeGameCodes.Contains(gameCode));
            }

            public Task CloseGameAsync(string gameCode)
            {
                if (!activeGameCodes.Contains(gameCode))
                {
                    throw new KeyNotFoundException($"Cannot close game. Game code {gameCode} does not exist.");
                }

                activeGameCodes.Remove(gameCode);
                return Task.CompletedTask;
            }
        }
    }
}
