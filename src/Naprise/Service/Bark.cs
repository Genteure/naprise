using Flurl;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Bark", "bark", "barks", SupportText = true)]
    [NotificationServiceWebsite("https://github.com/Finb/Bark")]
    [NotificationServiceApiDoc("https://github.com/Finb/bark-server/blob/master/docs/API_V2.md")]
    public sealed class Bark : NotificationService
    {
        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string token;
        internal readonly string? click_url;
        internal readonly string? group;
        internal readonly string? icon;
        internal readonly string? level;
        internal readonly string? sound;

        public Bark(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // bark://{host}/{token}
            // bark://{host}:{port}/{token}
            // barks://{host}/{token}
            // barks://{host}:{port}/{token}
            // bark://{host}/{token}?url={url}
            // bark://{host}/{token}?group={group}
            // bark://{host}/{token}?icon={icon}
            // bark://{host}/{token}?level={level}
            // bark://{host}/{token}?sound={sound}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid Bark URL. Expected format: bark://{host}/{token}");

            this.https = url.Scheme == "barks";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.token = segment[0];
            this.click_url = query.GetString("url");
            this.group = query.GetString("group");
            this.icon = query.GetString("icon");
            this.level = query.GetString("level");
            this.sound = query.GetString("sound");
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = new Payload
            {
                DeviceKey = token,
                Url = click_url,
                Group = group,
                Level = level,
                Icon = icon,
                Sound = sound,
            };

            var b = new StringBuilder();
            b.Append(this.Asset.GetAscii(message.Type));
            if (b.Length > 0)
                b.Append(' ');

            if (message.Title is not null)
            {
                b.AppendLine(message.Title);
                payload.Title = b.ToString();
                b.Clear();
            }

            if (payload.Title is not null)
            {
                payload.Body = message.PreferTextBody();
            }
            else
            {
                b.Append(message.PreferTextBody());
                payload.Body = b.ToString();
            }

            var req = new Url($"{(this.https ? "https" : "http")}://{this.hostAndPort}/").AppendPathSegment("push");
            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(req.ToString(), content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var respText = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Bark)}: {resp.StatusCode}")
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = respText,
                };
            }
        }

        private class Payload
        {
            public string? Title { get; set; }
            public string? Body { get; set; }
            public string? DeviceKey { get; set; }
            public string? Level { get; set; }
            public string? Sound { get; set; }
            public string? Icon { get; set; }
            public string? Group { get; set; }
            public string? Url { get; set; }
        }
    }
}
