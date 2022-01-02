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
        }

        public record PlayerView
        {
            public string? Name { get; init; }
        }
    }
}
