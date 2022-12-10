using Flurl;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("ServerChan", "serverchan", SupportText = true, SupportMarkdown = true)]
    [NotificationServiceWebsite("https://sct.ftqq.com/")]
    [NotificationServiceApiDoc("https://sct.ftqq.com/sendkey")]
    public sealed class ServerChan : NotificationService
    {
        internal readonly string token;
        internal readonly string? channel;
        internal readonly string? openid;

        public ServerChan(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // serverchan://{token}@serverchan
            // serverchan://{token}@serverchan?channel={channel}
            // serverchan://{token}@serverchan?openid={openid}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 0)
                throw new NapriseInvalidUrlException("Invalid ServerChan URL. Expected format: serverchan://{token}@serverchan");

            this.token = url.UserInfo;

            if (string.IsNullOrEmpty(this.token))
                throw new NapriseInvalidUrlException("Invalid ServerChan URL. Expected format: serverchan://{token}@serverchan");

            this.channel = query.GetString("channel");
            this.openid = query.GetString("openid");
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = new Payload
            {
                Title = message.GetTitleWithFallback(maxLengthFromBody: 32),
                Desp = message.PreferMarkdownBody(),
                Channel = channel,
                Openid = openid,
            };

            var url = new Url("https://sctapi.ftqq.com").AppendPathSegment($"{this.token}.send");
            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url.ToString(), content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var respText = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(ServerChan)}: {resp.StatusCode}")
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
            public string? Desp { get; set; }
            public string? Channel { get; set; }
            public string? Openid { get; set; }
        }
    }
}
