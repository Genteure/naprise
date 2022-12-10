using Flurl;
using System;
using System.Net.Http;

namespace Naprise
{
    public class ServiceConfig
    {
        public readonly Url Url;
        public readonly NapriseAsset Asset;
        public readonly Func<HttpClient> HttpClientFactory;

        public ServiceConfig(Url url, NapriseAsset asset, Func<HttpClient> httpClientFactory)
        {
            this.Url = url ?? throw new ArgumentNullException(nameof(url));
            this.Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            this.HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }
    }
}
