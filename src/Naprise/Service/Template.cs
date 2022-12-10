using Flurl;
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise.Service
{
    [NapriseNotificationService("Template", "", SupportText = true, SupportMarkdown = true, SupportHtml = false)] // TODO fill in blank
    [NotificationServiceWebsite("")] // TODO fill in blank
    [NotificationServiceApiDoc("")] // TODO fill in blank
    // TODO change visibility and class name
    internal sealed class Template : NotificationService
    {
        // TODO change class name
        public Template(ServiceConfig config) : base(config: config, bypassChecks: false)
        {

            var url = config.Url;
            var segment = url.PathSegments;
            var query = url.QueryParams;

            // fill in all the blanks and change visibility to public
            throw new InvalidProgramException("this is a template for adding new notification services");
        }

        public override async Task NotifyAsync(Message message, CancellationToken cancellationToken = default)
        {
            message.ThrowIfEmpty();

            // TODO build the message body

            var payload = new Payload
            {
                // TODO fill payload
                // TODO check message.Type
            };

            var url = new Url($"{(true ? "https" : "http")}://{"localhost"}").AppendPathSegments("example");
            var content = JsonContent.Create(payload, options: SharedJsonOptions.SnakeCaseNamingOptions);

            cancellationToken.ThrowIfCancellationRequested();
            var resp = await this.HttpClientFactory().PostAsync(url, content, cancellationToken);
            var respText = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Template)}: {resp.StatusCode}") // TODO change class name
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
                // TODO parse response and check if it's successful
                var status = jobj.RootElement.GetProperty("status").GetString();
                if (status != "ok")
                {
                    var respMessage = jobj.RootElement.GetProperty("message").GetString();
                    throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Template)}: \"{respMessage}\"")
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
                throw new NapriseNotifyFailedException($"Failed to send notification to {nameof(Template)}", ex)
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
            // TODO add payload
        }
    }
}
