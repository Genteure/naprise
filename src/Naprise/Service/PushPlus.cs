using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("PushPlus", "pushplus", SupportText = true, SupportMarkdown = true, SupportHtml = false)]
    [NotificationServiceWebsite("https://www.pushplus.plus/")]
    [NotificationServiceApiDoc("https://www.pushplus.plus/doc/guide/api.html")]
    public sealed class PushPlus : NotificationService
    {
        private static readonly IReadOnlyList<string> ValidChannels = new[] { "wechat", "webhook", "cp", "mail", "sms" };

        internal readonly string token;
        internal readonly string channel;

        public PushPlus(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // pushplus://{token}@{channel}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 0)
                throw new NapriseInvalidUrlException("Invalid PushPlus URL. Expected format: pushplus://{token}@{channel}");

            this.token = url.UserInfo;
            this.channel = url.Host;

            if (!ValidChannels.Contains(this.channel))
                throw new NapriseInvalidUrlException($"Invalid PushPlus URL. Channel must be one of: {string.Join(", ", ValidChannels)}");
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = new Payload
            {
                Token = token,
                Channel = channel,
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

            static void SelectBody(Message message, out string type, out string body)
            {
                if (message.Markdown is not null)
                {
                    type = "markdown";
                    body = message.Markdown;
                }
                else if (message.Text is not null)
                {
                    type = "txt";
                    body = message.Text;
                }
                else if (message.Html is not null)
                {
                    type = "html";
                    body = message.Html;
                }
                else
                {
                    type = "txt";
                    body = string.Empty;
                }
            }

            if (payload.Title is not null)
            {
                SelectBody(message, out var type, out var body);
                payload.Template = type;
                payload.Content = body;
            }
            else
            {
                SelectBody(message, out var type, out var body);
                payload.Template = type;
                if (type == "html")
                {
                    payload.Content = b.ToString();
                }
                else
                {
                    b.Append(body);
                    payload.Content = b.ToString();
                }
            }

            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync("https://www.pushplus.plus/send", content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var respText = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(PushPlus)}: {resp.StatusCode}")
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
            public string? Token { get; set; }
            public string? Title { get; set; }
            public string? Content { get; set; }
            public string? Template { get; set; }
            public string? Channel { get; set; }
        }
    }
}
