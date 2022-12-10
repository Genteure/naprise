using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Naprise.Tests")]

namespace Naprise
{
    /// <summary>
    /// Specifies the URL Schemes and supported message formats of a notification service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NapriseNotificationServiceAttribute : Attribute
    {
        /// <summary>
        /// Specifies the URL Schemes and supported message formats of a notification service.
        /// </summary>
        /// <param name="schemes">URL Schemes for this notification service.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public NapriseNotificationServiceAttribute(string displayName, params string[] schemes)
        {
            if (schemes == null)
                throw new ArgumentNullException(nameof(schemes));

            if (schemes.Length == 0)
                throw new ArgumentException("At least one scheme must be specified.", nameof(schemes));

            foreach (var scheme in schemes)
            {
                if (scheme.ToLowerInvariant() != scheme)
                    throw new ArgumentException($"Scheme '{scheme}' must be lowercase.", nameof(schemes));
            }

            this.DisplayName = displayName;
            this.Schemes = schemes;
        }

        /// <summary>
        /// Display name of this notification service.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// URL Schemes for this notification service.
        /// </summary>
        public string[] Schemes { get; }

        /// <summary>
        /// Plain text support of this notification service. Currently only used for documentation purpose.
        /// </summary>
        public bool SupportText { get; set; } = false;
        /// <summary>
        /// Markdown support of this notification service. Currently only used for documentation purpose.
        /// </summary>
        public bool SupportMarkdown { get; set; } = false;
        /// <summary>
        /// HTML support of this notification service. Currently only used for documentation purpose.
        /// </summary>
        public bool SupportHtml { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NotificationServiceWebsiteAttribute : Attribute
    {
        public NotificationServiceWebsiteAttribute(string url)
        {
            this.Url = url;
        }

        public string Url { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NotificationServiceApiDocAttribute : Attribute
    {
        public NotificationServiceApiDocAttribute(string url)
        {
            this.Url = url;
        }

        public string Url { get; }
    }
}
