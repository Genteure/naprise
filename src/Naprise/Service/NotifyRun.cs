using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("notify.run", "notifyrun", "notifyruns", SupportText = true)]
    [NotificationServiceWebsite("https://notify.run/")]
    [NotificationServiceApiDoc("https://notify.run/")]
    public sealed class NotifyRun : NotificationService
    {
        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string userinfo;
        internal readonly string channel;

        public NotifyRun(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // notifyrun(s)://{host}/{channel}
            // notifyrun(s)://{host}:{port}/{channel}
            // notifyrun(s)://{user}@{host}/{channel}
            // notifyrun(s)://{user}@{host}:{port}/{channel}
            // notifyrun(s)://{user}:{password}@{host}/{channel}
            // notifyrun(s)://{user}:{password}@{host}:{port}/{channel}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid NotifyRun URL. Expected format: notifyrun(s)://{user}:{password}@{host}:{port}/{channel}");

            this.https = url.Scheme == "notifyruns";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.channel = segment[0];
            this.userinfo = url.UserInfo;
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var b = new StringBuilder();
            b.Append(this.Asset.GetAscii(message.Type));

            if (b.Length > 0)
                b.Append(' ');

            if (message.Title is not null)
                b.AppendLine(message.Title);

            b.Append(message.PreferTextBody());

            var url = $"{(this.https ? "https" : "http")}://{this.hostAndPort}/{this.channel}";

            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(b.ToString()));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(NotifyRun)}: {resp.StatusCode}")
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
