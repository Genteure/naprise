using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Telegram", "telegram", SupportText = true)]
    [NotificationServiceWebsite("https://telegram.org/")]
    [NotificationServiceApiDoc("https://core.telegram.org/bots/api#sendmessage")]
    public sealed class Telegram : NotificationService
    {
        private static readonly IReadOnlyList<string> ValidParseMode = new[] { "MarkdownV2", "HTML", "Markdown" };

        internal readonly string token;
        internal readonly string chat_id;
        internal readonly string api_host;
        internal readonly string? message_thread_id;
        internal readonly string? parse_mode;
        internal readonly bool disable_web_page_preview;
        internal readonly bool disable_notification;
        internal readonly bool protect_content;

        public Telegram(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // telegram://{token}@{chat_id}
            // telegram://{token}@{chat_id}?api_host={api_host}
            // telegram://{token}@{chat_id}?message_thread_id={message_thread_id}
            // telegram://{token}@{chat_id}?parse_mode={parse_mode}
            // telegram://{token}@{chat_id}?disable_web_page_preview={disable_web_page_preview}
            // telegram://{token}@{chat_id}?disable_notification={disable_notification}
            // telegram://{token}@{chat_id}?protect_content={protect_content}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 0)
                throw new NapriseInvalidUrlException("Invalid Telegram URL. Expected format: telegram://{token}@{chat_id}");

            this.token = url.UserInfo;
            if (string.IsNullOrWhiteSpace(this.token))
                throw new NapriseInvalidUrlException("Invalid Telegram URL. Expected format: telegram://{token}@{chat_id}");


            var host = url.Host;
            if (string.IsNullOrWhiteSpace(host))
                throw new NapriseInvalidUrlException("Invalid Telegram URL. Expected format: telegram://{token}@{chat_id}");
            this.chat_id = long.TryParse(host, out var _) ? host : "@" + host;

            this.api_host = query.GetString("api_host") ?? "https://api.telegram.org";
            this.message_thread_id = query.GetString("message_thread_id");
            this.parse_mode = query.GetString("parse_mode");

            if (this.parse_mode is not null && !ValidParseMode.Any(x => x.Equals(this.parse_mode, StringComparison.OrdinalIgnoreCase)))
                throw new NapriseInvalidUrlException("Invalid Telegram URL. parse_mode must be one of the following: " + string.Join(", ", ValidParseMode));

            this.disable_web_page_preview = query.GetBool("disable_web_page_preview") ?? false;
            this.disable_notification = query.GetBool("disable_notification") ?? false;
            this.protect_content = query.GetBool("protect_content") ?? false;
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var b = new StringBuilder();
            b.Append(this.Asset.GetAscii(message.Type));

            if (b.Length > 0)
                b.Append(' ');

            if (message.Title is not null)
                b.AppendLine(message.Title);

            if (this.parse_mode is null)
            {
                b.AppendLine(message.PreferTextBody());
            }
            else if (this.parse_mode.StartsWith("markdown", StringComparison.OrdinalIgnoreCase))
            {
                b.AppendLine(message.PreferMarkdownBody());
            }
            else
            {
                if (message.Html is not null)
                    b.AppendLine(message.Html);
                else
                    b.AppendLine(message.PreferTextBody());
            }

            var payload = new Payload
            {
                ChatId = this.chat_id,
                MessageThreadId = this.message_thread_id,
                Text = b.ToString(),
                ParseMode = this.parse_mode,
                DisableWebPagePreview = this.disable_web_page_preview,
                DisableNotification = this.disable_notification,
                ProtectContent = this.protect_content,
            };

            var url = new Url(this.api_host).AppendPathSegments($"bot{this.token}", "sendMessage").ToString();
            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingIngoreNullOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Telegram)}: {resp.StatusCode}")
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
            public string? ChatId { get; set; }
            public string? MessageThreadId { get; set; }
            public string? Text { get; set; }
            public string? ParseMode { get; set; }
            public bool DisableWebPagePreview { get; set; }
            public bool DisableNotification { get; set; }
            public bool ProtectContent { get; set; }
        }
    }
}
