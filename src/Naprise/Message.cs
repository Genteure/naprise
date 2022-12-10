using AngleSharp;
using Markdig;
using System;
using System.Net;

namespace Naprise
{
    public interface IMessage
    {
        string? Title { get; }
        MessageType Type { get; }
        string? Text { get; }
        string? Markdown { get; }
        string? Html { get; }
    }

    /// <summary>
    /// Contains the notification message title and body.
    /// <br/>
    /// <see cref="Text"/>, <see cref="Markdown"/>, and <see cref="Html"/> should be the exact same content in different formats.
    /// </summary>
    public class Message : IMessage
    {
        public Message() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="title">Message title. Optional.</param>
        /// <param name="type">Type of the message.</param>
        /// <param name="text">Message body in plain text. Optional.</param>
        /// <param name="markdown">Message body in Markdown. Optional.</param>
        /// <param name="html">Message body in HTML. Optional.</param>
        public Message(MessageType type = MessageType.None, string? title = null, string? text = null, string? markdown = null, string? html = null)
        {
            this.Title = title;
            this.Type = type;
            this.Text = text;
            this.Markdown = markdown;
            this.Html = html;
        }

        public string? Title { get; set; }
        public MessageType Type { get; set; }
        public string? Text { get; set; }
        public string? Markdown { get; set; }
        public string? Html { get; set; }

        public static implicit operator Message(string? title)
        {
            return new Message(title: title);
        }

        public void ThrowIfEmpty()
        {
            if (this.Title is null && this.Text is null && this.Markdown is null && this.Html is null)
                throw new NapriseEmptyMessageException("Notification message is empty.");
        }
    }

    public static class NotificationMessageExtensions
    {
        private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseEmojiAndSmiley().Build();

        /// <summary>
        /// Get the message title, or create a title from message body as fallback if title is <see langword="null"/>
        /// </summary>
        /// <param name="message">The payload</param>
        /// <param name="maxLengthFromBody">Maximum title length if a title is being generated from body. <see cref="Message.Title"/> is always returned as is.</param>
        /// <returns></returns>
        public static string GetTitleWithFallback(this IMessage message, int maxLengthFromBody = 20)
        {
            if (maxLengthFromBody < 1)
                throw new ArgumentOutOfRangeException("maxLengthFromBody must be a positive number", nameof(maxLengthFromBody));

            if (message.Title is not null)
                return message.Title;

            if (message.Text is string text)
            {
                // search the end of first line
                var len = text.IndexOfAny(new[] { '\n', '\r' });
                // use length of the text if there's only a single line
                if (len < 1)
                    len = text.Length;
                // cap max length
                len = Math.Min(maxLengthFromBody, len);

                return text.Substring(0, len);
            }

            if (message.Markdown is string markdown)
            {
                // find the index after markdown header (e.g. #, ##, ###, etc.)
                var start = 0;
                while (start < markdown.Length && markdown[start] == '#')
                    start++;

                // skip whitespace
                while (start < markdown.Length && char.IsWhiteSpace(markdown[start]))
                    start++;

                // search the end of first line
                var len = markdown.IndexOfAny(new[] { '\n', '\r' });

                // use length of the text if there's only a single line
                if (len < 1)
                    len = markdown.Length;

                // cap max length
                len = Math.Min(maxLengthFromBody + start, len);

                return markdown.Substring(start, len - start);
            }

            if (message.Html is not null)
            {
                // we're just reading directly from memory, hopefully this won't cause any deadlocks...
                var document = BrowsingContext.New().OpenAsync(req => req.Content(message.Html)).GetAwaiter().GetResult();
                var tc = document.Body?.TextContent ?? string.Empty;
                return tc.Length > maxLengthFromBody ? tc.Substring(0, maxLengthFromBody) : tc;
            }

            return string.Empty;
        }

        public static string? PreferTextBody(this IMessage message)
        {
            if (message.Text is not null)
                return message.Text;

            if (message.Markdown is not null)
                return Markdown.ToPlainText(message.Markdown, pipeline);

            if (message.Html is not null)
            {
                // we're just reading directly from memory, hopefully this won't cause any deadlocks...
                var document = BrowsingContext.New().OpenAsync(req => req.Content(message.Html)).GetAwaiter().GetResult();
                return document.Body?.TextContent;
            }

            return null;
        }

        public static string? PreferMarkdownBody(this IMessage message)
        {
            if (message.Markdown is not null)
                return message.Markdown;

            if (message.Text is not null)
                return message.Text;

            if (message.Html is not null)
            {
                // returns plain text even though markdown is preferred, good enough for now
                var document = BrowsingContext.New().OpenAsync(req => req.Content(message.Html)).GetAwaiter().GetResult();
                return document.Body?.TextContent;
            }

            return null;
        }

        public static string? PreferHtmlBody(this IMessage message)
        {
            if (message.Html is not null)
                return message.Html;

            if (message.Markdown is not null)
                return Markdown.ToHtml(message.Markdown, pipeline);

            if (message.Text is not null)
                return WebUtility.HtmlEncode(message.Text);

            return null;
        }

        public static Message GenerateAllBodyFormats(this Message message)
        {
            message.Html ??= message.PreferHtmlBody();
            message.Markdown ??= message.PreferMarkdownBody();
            message.Text ??= message.PreferTextBody();
            return message;
        }
    }
}
