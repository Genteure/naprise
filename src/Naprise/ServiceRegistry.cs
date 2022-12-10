using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;

namespace Naprise
{
    public sealed class ServiceRegistry
    {
        private static readonly Type[] constructorArgumentTypes = new[] { typeof(ServiceConfig) };
        private static readonly ParameterExpression serviceConfigParameterExpression = Expression.Parameter(typeof(ServiceConfig), "config");

        public static readonly IReadOnlyList<Type> DefaultServices;

        static ServiceRegistry()
        {
            DefaultServices = typeof(ServiceRegistry).Assembly.GetTypes()
                .Where(t => t.Namespace == "Naprise.Service"
                            && t.IsPublic
                            && t.IsSealed
                            && t.IsSubclassOf(typeof(NotificationService))
                            && t.GetCustomAttribute<NapriseNotificationServiceAttribute>() != null)
                .ToArray();
        }

        private readonly Dictionary<string, Func<ServiceConfig, INotifier>> services = new();

        public ServiceRegistry()
        {
        }

        /// <summary>
        /// Silently ignore invalid URLs, return <see cref="Naprise.NoopNotifier"/> instead of throwing <see cref="NapriseUnknownSchemeException"/>.
        /// Default is <see langword="false"/>.
        /// </summary>
        public bool IgnoreUnknownScheme { get; set; } = false;

        /// <summary>
        /// Silently ignore invalid URLs, catches <see cref="NapriseInvalidUrlException"/> and returns <see cref="Naprise.NoopNotifier"/> instead.
        /// Default is <see langword="false"/>.
        /// </summary>
        public bool IgnoreInvalidUrl { get; set; } = false;

        /// <summary>
        /// Set the <see cref="System.Net.Http.HttpClient"/> for this registry,
        /// will use <see cref="Naprise.DefaultHttpClient"/> by default.
        /// </summary>
        public HttpClient? HttpClient { get; set; }

        public NapriseAsset Asset { get; set; } = new();

        public ServiceRegistry AddDefaultServices()
        {
            foreach (var type in DefaultServices)
                this.Add(type);

            return this;
        }

        public ServiceRegistry Add<T>() where T : INotifier => this.Add(typeof(T));

        public ServiceRegistry Add(Type service)
        {
            if (!typeof(INotifier).IsAssignableFrom(service))
                throw new ArgumentException("Service must be a INotifier", nameof(service));

            var attr = service.GetCustomAttribute<NapriseNotificationServiceAttribute>();
            if (attr == null)
                throw new ArgumentException("Service must have a NapriseNotificationServiceAttribute", nameof(service));

            var ctor = service.GetConstructor(constructorArgumentTypes);
            if (ctor == null)
                throw new ArgumentException("Service must have a constructor with a single Naprise.NotificationServiceConfig argument", nameof(service));

            var expr = Expression.Lambda(Expression.New(ctor, serviceConfigParameterExpression), serviceConfigParameterExpression);
            var factory = (Func<ServiceConfig, INotifier>)expr.Compile();

            foreach (var scheme in attr.Schemes)
                this.services[scheme] = factory;

            return this;
        }

        public INotifier Create(IEnumerable<Url> serviceUrls)
        {
            if (serviceUrls is null)
                throw new ArgumentNullException(nameof(serviceUrls));

            var urls = serviceUrls as IReadOnlyList<Url> ?? serviceUrls.ToList();

            if (urls.Count == 0)
                return Naprise.NoopNotifier;

            if (urls.Count == 1)
            {
                try
                {
                    // fast path for single service
                    var url = urls[0];
                    var notifier = this.NewNotifier(url);
                    if (notifier is not null)
                    {
                        return notifier;
                    }
                    else if (!this.IgnoreUnknownScheme)
                    {
                        var scheme = url.Scheme.ToLowerInvariant();
                        throw new NapriseUnknownSchemeException($"\"{scheme}://\" is not registered with this ServiceRegistry");
                    }
                    else
                    {
                        return Naprise.NoopNotifier;
                    }
                }
                catch (NapriseInvalidUrlException)
                {
                    if (this.IgnoreInvalidUrl)
                        return Naprise.NoopNotifier;
                    else
                        throw;
                }
            }

            // multiple services
            var services = new List<INotifier>(urls.Count);

            foreach (var url in urls)
            {
                try
                {
                    var notifier = this.NewNotifier(url);

                    if (notifier is not null)
                    {
                        services.Add(notifier);
                    }
                    else if (!this.IgnoreUnknownScheme)
                    {
                        var scheme = url.Scheme.ToLowerInvariant();
                        throw new NapriseUnknownSchemeException($"\"{scheme}://\" is not registered with this ServiceRegistry");
                    }
                }
                catch (NapriseInvalidUrlException)
                {
                    if (!this.IgnoreInvalidUrl)
                        throw;
                }
            }

            return services.Count switch
            {
                0 => Naprise.NoopNotifier,
                1 => services[0],
                _ => new CompositeNotifier(services)
            };
        }

        private INotifier? NewNotifier(Url url)
        {
            var scheme = url.Scheme;

            if (scheme is "http" or "https")
            {/*
                if (this.httpHostServiceConstructors.TryGetValue(url.Host.ToLowerInvariant(), out var constructor))
                {
                    var config = new ServiceConfig(url: url, mode: UrlParsingMode.Http, asset: this.Asset, httpClientFactory: this.GetHttpClient);
                    return (INotifier)constructor.Invoke(new object[] { config });
                }
                else
                {
                }*/
                return null;
            }
            else if (this.services.TryGetValue(scheme, out var factory))
            {
                var config = new ServiceConfig(url: url, asset: this.Asset, httpClientFactory: this.GetHttpClient);
                return factory(config);
            }
            else
            {
                return null;
            }
        }

        private HttpClient GetHttpClient() => this.HttpClient ?? Naprise.DefaultHttpClient;
    }

    public static class ServiceRegistryExtensions
    {
        public static INotifier Create(this ServiceRegistry registry, params string[] urls)
            => registry.Create(serviceUrls: urls.Where(x => !string.IsNullOrEmpty(x)).Select(x => new Url(x)));

        public static INotifier Create(this ServiceRegistry registry, IEnumerable<string> urls)
            => registry.Create(serviceUrls: urls.Where(x => !string.IsNullOrEmpty(x)).Select(x => new Url(x)));

        public static INotifier Create(this ServiceRegistry registry, params Url[] urls)
            => registry.Create(serviceUrls: urls);
    }
}
