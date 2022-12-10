using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("ntfy.sh", "ntfy", "ntfys", SupportText = true)]
    [NotificationServiceWebsite("https://ntfy.sh/")]
    [NotificationServiceApiDoc("https://docs.ntfy.sh/publish/")]
    public sealed class Ntfy : NotificationService
    {
        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string userinfo;
        internal readonly string topic;
        internal readonly string[] tags;
        internal readonly int? priority;
        internal readonly string? click;
        internal readonly string? delay;
        internal readonly string? email;

        public Ntfy(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // ntfy(s)://{user}:{password}@{host}:{port}/{topic}
            // ntfy://{host}/{topic}
            // ntfys://{host}/{topic}
            // ntfy://{user:password}@{host}/{topic}
            // ntfy://{host}:{port}/{topic}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid Ntfy URL. Expected format: ntfy(s)://{user}:{password}@{host}:{port}/{topic}");

            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.topic = segment[0];

            if (string.IsNullOrWhiteSpace(this.topic))
                throw new NapriseInvalidUrlException("Invalid Ntfy URL. Expected format: ntfy(s)://{user}:{password}@{host}:{port}/{topic}");

            this.https = url.Scheme == "ntfys";
            this.userinfo = url.UserInfo;
            this.tags = query.GetStringArray("tags");
            this.priority = query.GetInt("priority");
            this.click = query.GetString("click");
            this.delay = query.GetString("delay");
            this.email = query.GetString("email");
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = new Payload
            {
                Topic = topic,
                Message = message.PreferTextBody(),
                Title = message.Title,
                Tags = tags,
                Priority = priority,
                Click = click,
                Delay = delay,
                Email = email,
            };

            var req = new HttpRequestMessage(HttpMethod.Post, $"{(this.https ? "https" : "http")}://{this.hostAndPort}")
            {
                Content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingOptions)
            };

            if (!string.IsNullOrEmpty(this.userinfo))
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.userinfo)));

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().SendAsync(req, cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to Ntfy: {resp.StatusCode}")
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
            public string? Topic { get; set; }
            public string? Message { get; set; }
            public string? Title { get; set; }
            public string[]? Tags { get; set; }
            public int? Priority { get; set; }
            public string? Click { get; set; }
            public string? Delay { get; set; }
            public string? Email { get; set; }
        }
    }
}
