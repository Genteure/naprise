using Flurl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("PushDeer", "pushdeer", "pushdeers", SupportText = true, SupportMarkdown = true)]
    [NotificationServiceWebsite("https://www.pushdeer.com/")]
    [NotificationServiceApiDoc("https://www.pushdeer.com/dev.html#%E6%8E%A8%E9%80%81%E6%B6%88%E6%81%AF")]
    public sealed class PushDeer : NotificationService
    {
        internal readonly bool https;
        internal readonly string hostAndPort;
        internal readonly string userinfo;
        internal readonly string pushkey;

        public PushDeer(ServiceConfig config) : base(config: config, bypassChecks: false)
        {
            // pushdeer://{host}/{pushkey}
            // pushdeers://{host}/{pushkey}
            // pushdeer://{host}:{port}/{pushkey}
            // pushdeers://{host}:{port}/{pushkey}
            // pushdeer://{userinfo}@{host}/{pushkey}
            // pushdeers://{userinfo}@{host}/{pushkey}
            // pushdeer://{userinfo}@{host}:{port}/{pushkey}
            // pushdeers://{userinfo}@{host}:{port}/{pushkey}

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            if (segment.Count != 1)
                throw new NapriseInvalidUrlException("Invalid PushDeer URL. Expected format: pushdeer://{host}/{pushkey}");

            this.https = url.Scheme == "pushdeers";
            this.hostAndPort = url.Port is null ? url.Host : $"{url.Host}:{url.Port}";
            this.userinfo = url.UserInfo;
            this.pushkey = segment[0];
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            var b = new StringBuilder();
            b.Append(this.Asset.GetAscii(message.Type));
            if (b.Length > 0)
                b.Append(' ');

            string? title = null;
            if (message.Title is not null)
            {
                b.AppendLine(message.Title);
                title = b.ToString();
                b.Clear();
            }

            bool isMarkdown;

            if (message.Markdown is not null)
            {
                isMarkdown = true;
                b.Append(message.Markdown);
            }
            else
            {
                isMarkdown = false;
                b.Append(message.PreferTextBody());
            }

            var url = new Url($"{(this.https ? "https" : "http")}://{this.hostAndPort}").AppendPathSegments("message", "push").SetQueryParams(new
            {
                this.pushkey,
                type = isMarkdown ? "markdown" : "text",
                text = title ?? b.ToString(),
            });
            if (title is not null)
                url.SetQueryParam("desp", b.ToString());

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url.ToString(), null, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var respText = await resp.Content.ReadAsStringAsync();
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(PushDeer)}: {resp.StatusCode}")
                {
                    Notifier = this,
                    Notification = message,
                    ResponseStatus = resp.StatusCode,
                    ResponseBody = respText,
                };
            }
        }
    }
}
