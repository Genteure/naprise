using Flurl;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Apprise", "apprise", "apprises", SupportText = true, SupportMarkdown = true, SupportHtml = true)]
    [NotificationServiceWebsite("https://github.com/caronc/apprise-api")]
    [NotificationServiceApiDoc("https://github.com/caronc/apprise-api#persistent-storage-solution")]
    public sealed class Apprise : NotificationService
    {
        internal readonly bool https;
        internal readonly string userinfo;
        internal readonly string hostAndPort;
        internal readonly string token;
        internal readonly Format format = Format.Unknown;
        internal readonly string? tag;

        public Apprise(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // apprise(s)://{user}:{password}@{host}:{port}/{token}

            var url = config.Url;
            var segment = url.PathSegments;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid Apprise URL. Expected format: apprise(s)://{user}:{password}@{host}:{port}/{token}");

            this.token = segment[0];

            if (string.IsNullOrWhiteSpace(this.token))
                throw new NapriseInvalidUrlException("Invalid Apprise URL. Expected format: apprise(s)://{user}:{password}@{host}:{port}/{token}");

            this.https = url.Scheme == "apprises";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.userinfo = url.UserInfo;
            this.tag = url.QueryParams.GetString("tag");
            this.format = url.QueryParams.GetString("format") switch
            {
                "text" => Format.Text,
                "markdown" => Format.Markdown,
                "html" => Format.Html,
                _ => Format.Unknown,
            };
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var payload = new Payload
            {
                Title = message.Title,
                Body = this.format switch
                {
                    Format.Text => message.PreferTextBody(),
                    Format.Markdown => message.PreferMarkdownBody(),
                    Format.Html => message.PreferHtmlBody(),
                    _ => message.PreferMarkdownBody(),
                },
                Format = this.format switch
                {
                    Format.Text => "text",
                    Format.Markdown => "markdown",
                    Format.Html => "html",
                    _ => "markdown",
                },
                Type = message.Type switch
                {
                    MessageType.Info => "info",
                    MessageType.Success => "success",
                    MessageType.Warning => "warning",
                    MessageType.Error => "failure",
                    MessageType.None or _ => null,
                },
                Tag = this.tag
            };

            var target = new Url("http://" + this.hostAndPort)
                .AppendPathSegment("notify")
                .AppendPathSegment(this.token);

            if (this.https)
                target.Scheme = "https";

            var req = new HttpRequestMessage(method: HttpMethod.Post, requestUri: target.ToString())
            {
                Content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingOptions),
            };

            if (!string.IsNullOrEmpty(this.userinfo))
                req.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.userinfo)));

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().SendAsync(req, cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to Apprise: {resp.StatusCode}")
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
            public string? Body { get; set; }
            public string? Title { get; set; }
            public string? Type { get; set; }
            public string? Tag { get; set; }
            public string? Format { get; set; }
        }
    }
}
