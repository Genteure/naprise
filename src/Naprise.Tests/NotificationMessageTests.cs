namespace Naprise.Tests
{
    public class NotificationMessageTests
    {
        [Theory]
        [InlineData("Hello World")]
        [InlineData("super duper duper duper duper loooooooong")]
        public void GetTitleFromTitle(string title)
        {
            var message = new Message(title: title);
            Assert.Equal(title, message.GetTitleWithFallback(maxLengthFromBody: 20));
        }

        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("Hello World\nBBBBBBBBB", "Hello World")]
        [InlineData("Hello World\rBBBBBBBBB", "Hello World")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAbbbbbbb", "AAAAAAAAAAAAAAAAAAAA")]
        public void GetTitleFromText(string text, string title)
        {
            var message = new Message(text: text);
            Assert.Equal(title, message.GetTitleWithFallback(maxLengthFromBody: 20));
        }

        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("#Hello World", "Hello World")]
        [InlineData("# Hello World", "Hello World")]
        [InlineData("##Hello World", "Hello World")]
        [InlineData("## Hello World", "Hello World")]
        [InlineData("##### Hello World", "Hello World")]
        [InlineData("##### 你好！", "你好！")]
        [InlineData("#                     Hello World", "Hello World")]
        [InlineData("#                     Hello World\nAAAAA", "Hello World")]
        [InlineData("##### Title\nBody", "Title")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAbbbbbbb", "AAAAAAAAAAAAAAAAAAAA")]
        [InlineData("AAAAAAAAAAAAAAAAAAAAbbbbbbb\nBBBB", "AAAAAAAAAAAAAAAAAAAA")]
        [InlineData("#               AAAAAAAAAAAAAAAAAAAAbbbbbbb", "AAAAAAAAAAAAAAAAAAAA")]
        [InlineData("#               AAAAAAAAAAAAAAAAAAAAbbbbbbb\nBBBBB", "AAAAAAAAAAAAAAAAAAAA")]
        public void GetTitleFromMarkdown(string markdown, string title)
        {
            var message = new Message(markdown: markdown);
            Assert.Equal(title, message.GetTitleWithFallback(maxLengthFromBody: 20));
        }

        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("<div><div><div>Hi!</div><div>Lorem ipsum dolor</div><div>Lorem ipsum dolor</div><div>Lorem ipsum dolor</div></div></div>", "Hi!Lorem ipsum dolor")]
        public void GetTitleFromHtml(string html, string title)
        {
            var message = new Message(html: html);
            Assert.Equal(title, message.GetTitleWithFallback(maxLengthFromBody: 20));
        }

        [Theory]
        [InlineData("Hello World", "Hello World")]
        [InlineData("<h1>Hello World</h1>", "&lt;h1&gt;Hello World&lt;/h1&gt;")]
        [InlineData("<img>", "&lt;img&gt;")]
        [InlineData("<img src=\"https://example.com/\" alt=\"Hello World\">", "&lt;img src=&quot;https://example.com/&quot; alt=&quot;Hello World&quot;&gt;")]
        public void EnsureHtmlFromTextAreEscaped(string text, string html)
        {
            var message = new Message(text: text);
            Assert.Equal(html, message.PreferHtmlBody());
        }
    }
}