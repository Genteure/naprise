using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("OneBot 12", "onebot12", "onebot12s", SupportText = true)]
    [NotificationServiceWebsite("https://12.onebot.dev/")]
    [NotificationServiceApiDoc("https://12.onebot.dev/interface/message/actions/#send_message")]
    public sealed class OneBot12 : NotificationService
    {
        private static readonly string ContentType = "application/json";

        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string access_token;
        internal readonly string detail_type;
        internal readonly string? user_id;
        internal readonly string? group_id;
        internal readonly string? guild_id;
        internal readonly string? channel_id;

        public OneBot12(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // onebot12://{access_token}@{host}:{port}/private/{user_id}
            // onebot12://{access_token}@{host}:{port}/group/{group_id}
            // onebot12://{access_token}@{host}:{port}/channel/{guild_id}/{channel_id}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count is < 1 or > 3)
                throw new NapriseInvalidUrlException("Invalid OneBot12 URL. Expected format: onebot12://{access_token}@{host}:{port}/{detail_type}/{id}...");

            this.https = url.Scheme == "onebot12s";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.access_token = url.UserInfo;

            this.detail_type = segment[0];

            switch (this.detail_type)
            {
                case "private":
                    if (segment.Count != 2 || string.IsNullOrEmpty(segment[1]))
                        throw new NapriseInvalidUrlException("Invalid OneBot12 URL. Expected format: onebot12://{access_token}@{host}:{port}/private?user_id={user_id}");
                    this.user_id = segment[1];
                    break;
                case "group":
                    if (segment.Count != 2 || string.IsNullOrEmpty(segment[1]))
                        throw new NapriseInvalidUrlException("Invalid OneBot12 URL. Expected format: onebot12://{access_token}@{host}:{port}/group?group_id={group_id}");
                    this.group_id = segment[1];
                    break;
                case "channel":
                    if (segment.Count != 3 || string.IsNullOrEmpty(segment[1]) || string.IsNullOrEmpty(segment[2]))
                        throw new NapriseInvalidUrlException("Invalid OneBot12 URL. Expected format: onebot12://{access_token}@{host}:{port}/channel?guild_id={guild_id}&channel_id={channel_id}");
                    this.guild_id = segment[1];
                    this.channel_id = segment[2];
                    break;
                default:
                    throw new NapriseInvalidUrlException("Invalid OneBot12 URL. Supported detail_type: private, group, channel");
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
                Params = new Params
                {
                    DetailType = this.detail_type,
                    Message = new Msg[1],
                }
            };

            switch (this.detail_type)
            {
                case "private":
                    payload.Params.UserId = this.user_id;
                    break;
                case "group":
                    payload.Params.GroupId = this.group_id;
                    break;
                case "channel":
                    payload.Params.ChannelId = this.channel_id;
                    payload.Params.GuildId = this.guild_id;
                    break;
                default:
                    break;
            }

            payload.Params.Message[0] = new Msg
            {
                Type = "text",
                Data = new MsgData
                {
                    Text = b.ToString(),
                }
            };

            var url = $"{(this.https ? "https" : "http")}://{this.hostAndPort}";

            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentType);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            var respText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(OneBot12)}: {resp.StatusCode}")
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
                if (status != "ok")
                {
                    var respMessage = jobj.RootElement.GetProperty("message").GetString();
                    throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(OneBot12)}: \"{respMessage}\"")
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
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(OneBot12)}", ex)
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
            public string Action { get; set; } = "send_message";
            public Params? Params { get; set; }
        }

        private class Params
        {
            public string? DetailType { get; set; }
            public string? UserId { get; set; }
            public string? GroupId { get; set; }
            public string? GuildId { get; set; }
            public string? ChannelId { get; set; }
            public Msg[]? Message { get; set; }
        }

        private class Msg
        {
            public string? Type { get; set; }
            public MsgData? Data { get; set; }

        }

        private class MsgData
        {
            public string? Text { get; set; }
        }
    }
}
