using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise
{
    public abstract class NotificationService : INotifier
    {
        protected NapriseAsset Asset { get; }
        protected Func<HttpClient> HttpClientFactory { get; }

        protected NotificationService(ServiceConfig config, bool bypassChecks = false)
        {
            this.Asset = config.Asset;
            this.HttpClientFactory = config.HttpClientFactory;

            if (bypassChecks)
                return;

            var schemes = this.GetType().GetCustomAttribute<NapriseNotificationServiceAttribute>()?.Schemes;

            if (schemes is null)
                throw new NapriseInvalidUrlException($"Service \"{this.GetType().Name}\" does not have a NapriseNotificationServiceAttribute, this should not happen.");

            if (!schemes.Contains(config.Url.Scheme))
                throw new NapriseInvalidUrlException($"Service \"{this.GetType().Name}\" does not support URL scheme \"{config.Url.Scheme}\".");
        }

        public abstract Task NotifyAsync(Message message, CancellationToken cancellationToken = default);
    }
}
