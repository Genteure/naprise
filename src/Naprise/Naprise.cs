using Flurl;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Naprise
{
    public static class Naprise
    {
        public static HttpClient DefaultHttpClient { get; set; } = new HttpClient();

        public static ServiceRegistry DefaultRegistry { get; set; } = new ServiceRegistry().AddDefaultServices();

        /// <summary>
        /// A <see cref="INotifier"/> that does nothing.
        /// </summary>
        public static INotifier NoopNotifier { get; } = new Noop();

        public static INotifier Create(params string[] urls) => DefaultRegistry.Create(urls);
        public static INotifier Create(params Url[] urls) => DefaultRegistry.Create(urls);
        public static INotifier Create(IEnumerable<string> urls) => DefaultRegistry.Create(urls);
        public static INotifier Create(IEnumerable<Url> urls) => DefaultRegistry.Create(urls);

        private class Noop : INotifier
        {
            public Task NotifyAsync(Message message, CancellationToken cancellationToken = default) => Task.CompletedTask;
        }
    }
}
