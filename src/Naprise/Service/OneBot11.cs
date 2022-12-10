using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("OneBot 11", "onebot11", "onebot11s", SupportText = true)]
    [NotificationServiceWebsite("https://11.onebot.dev/")]
    [NotificationServiceApiDoc("https://github.com/botuniverse/onebot-11/blob/master/api/public.md#send_msg-%E5%8F%91%E9%80%81%E6%B6%88%E6%81%AF")]
    public sealed class OneBot11 : NotificationService
    {
        private static readonly string ContentType = "application/json";

        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string access_token;
        internal readonly string message_type;
        internal readonly string? user_id;
        internal readonly string? group_id;

        public OneBot11(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // onebot11://{access_token}@{host}:{port}/private/{user_id}
            // onebot11://{access_token}@{host}:{port}/group/{group_id}

            var url = config.Url;
            var segment = url.PathSegments;

            if (segment.Count != 2)
                throw new NapriseInvalidUrlException("Invalid OneBot11 URL. Expected format: onebot11://{access_token}@{host}:{port}/{private_or_group}/{id}");

            this.https = url.Scheme == "onebot11s";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.access_token = url.UserInfo;

            this.message_type = segment[0];

            switch (this.message_type)
            {
                case "private":
                    this.user_id = segment[1];
                    if (string.IsNullOrEmpty(this.user_id))
                        throw new NapriseInvalidUrlException("Invalid OneBot11 URL. Expected format: onebot11://{access_token}@{host}:{port}/private/{user_id}");
                    break;
                case "group":
                    this.group_id = segment[1];
                    if (string.IsNullOrEmpty(this.group_id))
                        throw new NapriseInvalidUrlException("Invalid OneBot11 URL. Expected format: onebot11://{access_token}@{host}:{port}/group/{group_id}");
                    break;
                default:
                    throw new NapriseInvalidUrlException("Invalid OneBot11 URL. Supported message_type: private, group");
            }
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

            var payload = new Payload
            {
                MessageType = this.message_type,
                Message = b.ToString(),
            };

            switch (this.message_type)
            {
                case "private":
                    payload.UserId = this.user_id;
                    break;
                case "group":
                    payload.GroupId = this.group_id;
                    break;
                default:
                    break;
            }

            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);
            var url = $"{(this.https ? "https" : "http")}://{this.hostAndPort}";

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            var respText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(OneBot11)}: {resp.StatusCode}")
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = respText,
                };
            }

            try
            {
                var jobj = JsonDocument.Parse(respText);
                var status = jobj.RootElement.GetProperty("status").GetString();
                if (status is not "ok" and not "async")
                {
                    var respMessage = jobj.RootElement.GetProperty("retcode").GetString();
                    throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(OneBot11)}: \"{respMessage}\"")
                    {
                        Notifier = this,
                        Notification = message,
                        ResponseStatus = resp.StatusCode,
                        ResponseBody = respText,
                    };
                }
            }
            catch (Exception ex)
            {
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(OneBot11)}", ex)
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
            public string? MessageType { get; set; }
            public string? UserId { get; set; }
            public string? GroupId { get; set; }
            public string? Message { get; set; }
        }
    }
}
