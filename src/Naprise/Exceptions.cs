using System;
using System.Net;

namespace Naprise
{
    /// <summary>
    /// Base exception for Naprise
    /// </summary>
    [Serializable]
    public class NapriseException : Exception
    {
        public NapriseException() { }
        public NapriseException(string message) : base(message) { }
        public NapriseException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class NapriseUnknownSchemeException : NapriseException
    {
        public NapriseUnknownSchemeException() { }
        public NapriseUnknownSchemeException(string message) : base(message) { }
        public NapriseUnknownSchemeException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class NapriseInvalidUrlException : NapriseException
    {
        public NapriseInvalidUrlException() { }
        public NapriseInvalidUrlException(string message) : base(message) { }
        public NapriseInvalidUrlException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class NapriseEmptyMessageException : NapriseException
    {
        public NapriseEmptyMessageException() { }
        public NapriseEmptyMessageException(string message) : base(message) { }
        public NapriseEmptyMessageException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class NapriseNotifyFailedException : NapriseException
    {
        private const string NAPRISE_NOTIFIER = "NapriseNotifier";
        private const string NAPRISE_NOTIFICATION = "NapriseNotification";
        private const string NAPRISE_RESPONSE_BODY = "NapriseResponseBody";
        private const string NAPRISE_RESPONSE_STATUS_CODE = "NapriseResponseStatusCode";

        public INotifier? Notifier
        {
            get => this.Data.Contains(NAPRISE_NOTIFIER) ? (INotifier)this.Data[NAPRISE_NOTIFIER] : null;
            set => this.Data[NAPRISE_NOTIFIER] = value;
        }

        public Message? Notification
        {
            get => this.Data.Contains(NAPRISE_NOTIFICATION) ? (Message)this.Data[NAPRISE_NOTIFICATION] : null;
            set => this.Data[NAPRISE_NOTIFICATION] = value;
        }

        public HttpStatusCode? ResponseStatus
        {
            get => this.Data.Contains(NAPRISE_RESPONSE_STATUS_CODE) ? (HttpStatusCode)this.Data[NAPRISE_RESPONSE_STATUS_CODE] : null;
            set => this.Data[NAPRISE_RESPONSE_STATUS_CODE] = value;
        }

        public string? ResponseBody
        {
            get => this.Data.Contains(NAPRISE_RESPONSE_BODY) ? (string)this.Data[NAPRISE_RESPONSE_BODY] : null;
            set => this.Data[NAPRISE_RESPONSE_BODY] = value;
        }

        public NapriseNotifyFailedException() { }
        public NapriseNotifyFailedException(string message) : base(message) { }
        public NapriseNotifyFailedException(string message, Exception inner) : base(message, inner) { }
    }
}
