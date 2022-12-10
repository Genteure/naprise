using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Notica", "notica", "noticas", SupportText = true)]
    [NotificationServiceWebsite("https://notica.us/")]
    [NotificationServiceApiDoc("https://notica.us/")]
    public sealed class Notica : NotificationService
    {
        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string userinfo;
        internal readonly string token;

        public Notica(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // notica(s)://{host}/{token}
            // notica(s)://{host}:{port}/{token}
            // notica(s)://{user}@{host}/{token}
            // notica(s)://{user}@{host}:{port}/{token}
            // notica(s)://{user}:{password}@{host}/{token}
            // notica(s)://{user}:{password}@{host}:{port}/{token}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid Notica URL. Expected format: notica(s)://{user}:{password}@{host}:{port}/{token}");

            this.https = url.Scheme == "noticas";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.token = segment[0];
            this.userinfo = url.UserInfo;
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var b = new StringBuilder("d:");

            b.Append(this.Asset.GetAscii(message.Type));

            if (b.Length > 2)
                b.Append(' ');

            if (message.Title is not null)
                b.AppendLine(message.Title);

            b.Append(message.PreferTextBody());

            var url = $"{(this.https ? "https" : "http")}://{this.hostAndPort}/?{this.token}";

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(b.ToString()));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Notica)}: {resp.StatusCode}")
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = text,
                };
            }
        }
    }
}
