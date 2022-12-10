using Flurl;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Discord", "discord", SupportText = true, SupportMarkdown = true)]
    [NotificationServiceWebsite("https://discord.com")]
    [NotificationServiceApiDoc("https://discord.com/developers/docs/resources/webhook#execute-webhook")]
    public sealed class Discord : NotificationService
    {
        internal readonly string webhookId;
        internal readonly string webhookToken;

        internal readonly string? username;
        internal readonly string? avatarUrl;
        internal readonly bool? tts;

        public Discord(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            var url = config.Url;
            var segments = url.PathSegments;
            var queryParams = url.QueryParams;

            if (segments.Count != 1)
                throw new NapriseInvalidUrlException("Invalid Discord URL. Expected format: discord://{webhookId}/{webhookToken}");

            this.webhookId = url.Host;
            this.webhookToken = segments[0];

            this.username = queryParams.GetString("username");
            this.avatarUrl = queryParams.GetString("avatar_url");
            this.tts = queryParams.GetBool("tts", false);
        }

        // https://discord.com/developers/docs/resources/webhook#execute-webhook
        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            message.ThrowIfEmpty();

            var payload = new DiscordWebhookPayload
            {
                Username = this.username,
                AvatarUrl = this.avatarUrl,
                Tts = this.tts,
                Embeds = new Embed[1],
            };

            payload.Embeds[0] = new Embed
            {
                Title = message.Title,
                Description = message.PreferMarkdownBody(),
                Color = message.Type == MessageType.None ? null : this.Asset.GetColor(message.Type).Value,
            };

            var url = new Url("https://discord.com/api/webhooks/").AppendPathSegments(this.webhookId, this.webhookToken);
            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to Discord: {resp.StatusCode}")
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = text,
                };
            }
        }

        private class DiscordWebhookPayload
        {
            public string? AvatarUrl { get; set; }
            public string? Username { get; set; }
            public bool? Tts { get; set; }
            public string? Content { get; set; }
            public Embed[]? Embeds { get; set; }
        }

        private class Embed
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public int? Color { get; set; }
        }
    }
}
