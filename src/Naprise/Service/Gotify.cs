using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Gotify", "gotify", "gotifys", SupportText = true, SupportMarkdown = true)]
    [NotificationServiceWebsite("https://gotify.net/")]
    [NotificationServiceApiDoc("https://gotify.net/docs/pushmsg")]
    public sealed class Gotify : NotificationService
    {
        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string token;
        internal readonly int? priority;
        internal readonly string? clickUrl;

        public Gotify(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // gotify://{hostname}/{token}
            // gotifys://{hostname}/{token}
            // gotifys://{hostname}:{port}/{token}
            // gotifys://{hostname}/{path}/{token}
            // gotifys://{hostname}:{port}/{path}/{token}
            // gotifys://{hostname}/{token}/?priority=5

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid Gotify URL. Expected format: gotify(s)://{hostname}/{token}");

            this.https = url.Scheme == "gotifys";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.token = segment[0];

            if (string.IsNullOrWhiteSpace(this.token))
                throw new NapriseInvalidUrlException("Invalid Gotify URL. Expected format: gotify(s)://{hostname}/{token}");

            this.priority = query.GetInt("priority");
            this.clickUrl = query.GetString("click_url");
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = new Payload
            {
                Title = message.Title,
                Priority = priority,
                Extras = new Dictionary<string, Dictionary<string, object>>()
            };

            if (message.Markdown is not null)
            {
                payload.Message = message.Markdown;
                payload.Extras.Add("client::display", new Dictionary<string, object> { ["contentType"] = "text/markdown" });
            }
            else
            {
                payload.Message = message.PreferTextBody();
                payload.Extras.Add("client::display", new Dictionary<string, object> { ["contentType"] = "text/plain" });
            }

            if (payload.Message is null)
            {
                payload.Message = payload.Title ?? string.Empty;
                payload.Title = string.Empty;
            }

            payload.Title = message.Type switch
            {
                MessageType.None => payload.Title,
                MessageType t => this.Asset.GetAscii(t) + " " + payload.Title,
            };

            if (this.clickUrl is not null)
            {
                payload.Extras.Add("client::notification", new Dictionary<string, object> { ["click"] = new { url = this.clickUrl } });
            }

            var url = $"{(this.https ? "https" : "http")}://{this.hostAndPort}/message?token={this.token}";
            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Gotify)}: {resp.StatusCode}")
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = text,
                };
            }
        }

        private class Payload
        {
            public string? Title { get; set; }
            public string? Message { get; set; }
            public int? Priority { get; set; }
            public Dictionary<string, Dictionary<string, object>>? Extras { get; set; }
        }
    }
}
