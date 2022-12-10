using Naprise.Service;

namespace Naprise.Tests.Service;
public class TelegramTests
{
    [Theory]
    [InlineData("telegram://123456:ABC5e-7z_3@channelusername", "123456:ABC5e-7z_3", "@channelusername", "https://api.telegram.org", null, null, false, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", null, null, false, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@-654987", "123456:ABC5e-7z_3", "-654987", "https://api.telegram.org", null, null, false, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?api_host=https://api.example.com", "123456:ABC5e-7z_3", "654987", "https://api.example.com", null, null, false, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?message_thread_id=123", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", "123", null, false, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?parse_mode=Markdown", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", null, "Markdown", false, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?disable_web_page_preview", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", null, null, true, false, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?disable_notification", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", null, null, false, true, false)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?protect_content", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", null, null, false, false, true)]
    [InlineData("telegram://123456:ABC5e-7z_3@654987?disable_web_page_preview&disable_notification&protect_content", "123456:ABC5e-7z_3", "654987", "https://api.telegram.org", null, null, true, true, true)]
    public void TelegramUrlConverterTest(string url, string token, string chat_id, string api_host, string? message_thread_id, string? parse_mode, bool disable_web_page_preview, bool disable_notification, bool protect_content)
    {
        var telegram = new Telegram(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(token, telegram.token);
        Assert.Equal(chat_id, telegram.chat_id);
        Assert.Equal(api_host, telegram.api_host);
        Assert.Equal(message_thread_id, telegram.message_thread_id);
        Assert.Equal(parse_mode, telegram.parse_mode);
        Assert.Equal(disable_web_page_preview, telegram.disable_web_page_preview);
        Assert.Equal(disable_notification, telegram.disable_notification);
        Assert.Equal(protect_content, telegram.protect_content);
    }
}
