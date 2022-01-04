using Microsoft.AspNetCore.SignalR;

using WikipediaGame.Server.Hubs;

namespace WikipediaGame.Server.Hubs
{
    public class PlayHubHelper
    {
        private readonly IHubContext<PlayHub> hub;

        public PlayHubHelper(IHubContext<PlayHub> hub)
        {
            this.hub = hub;
        }

        public async Task UpdateAsync(string connectionId, ConnectionUpdate update)
        {
            await this.hub.Clients.Client(connectionId).SendAsync("Update", update);
        }

        public record ConnectionUpdate
        {
            public bool InGame { get; init; }

            public GameView? Game { get; init; }
        }

        public record GameView
        {
            public string? GameCode { get; init; }
            public IReadOnlyList<PlayerView>? Players { get; init; }
            public IReadOnlyList<string>? Events { get; init; }
            public bool InPlay { get; init; }
            public AnswerView? Answer { get; init; }
            public ArticleView? Article { get; init; }
            public bool Revealed { get; init; }
            public string? Clue { get; init; }
            public string? Username { get; init; }
            public IReadOnlyList<string>? Options { get; init; }
        }

        public record PlayerView
        {
            public string? Name { get; init; }
            public bool IsGuesser { get; init; }
            public bool HasArticle { get; init; }
        }

        public record ArticleView
        {
            public string? Id { get; init; }
            public string? Name { get; init; }
            public string? Description { get; init; }
            public string? Extract { get; init; }
        }

        public record AnswerView
        {
            public string? Username { get; init; }
            public ArticleView? Article { get; init; }
        }
    }
}
