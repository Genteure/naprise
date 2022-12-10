using Moq;
using Moq.Contrib.HttpClient;
using Naprise.Service;
using System.Net;

namespace Naprise.Tests.Service;
public class DiscordTests
{
    [Theory]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz", "1234567890", "abcdefghijklmnopqrstuvwxyz", null, null, false)]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz?username=TestUser", "1234567890", "abcdefghijklmnopqrstuvwxyz", "TestUser", null, false)]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz?avatar_url=https://example.com/avatar.png", "1234567890", "abcdefghijklmnopqrstuvwxyz", null, "https://example.com/avatar.png", false)]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz?tts", "1234567890", "abcdefghijklmnopqrstuvwxyz", null, null, true)]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz?tts=true", "1234567890", "abcdefghijklmnopqrstuvwxyz", null, null, true)]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz?tts=false", "1234567890", "abcdefghijklmnopqrstuvwxyz", null, null, false)]
    [InlineData("discord://1234567890/abcdefghijklmnopqrstuvwxyz?username=TestUser&avatar_url=https://example.com/avatar.png&tts=true", "1234567890", "abcdefghijklmnopqrstuvwxyz", "TestUser", "https://example.com/avatar.png", true)]
    public void TestUrlParsing(string url, string webhookId, string webhookToken, string? username, string? avatarUrl, bool? tts)
    {
        var service = new Discord(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(webhookId, service.webhookId);
        Assert.Equal(webhookToken, service.webhookToken);
        Assert.Equal(username, service.username);
        Assert.Equal(avatarUrl, service.avatarUrl);
        Assert.Equal(tts, service.tts);
    }

    [Theory]
    [InlineData("discord://1234567890")] // missing webhookToken
    [InlineData("discord://1234567890/aaa/bbb")] // too many path segments
    public void ExpectNapriseInvalidUrlException(string url)
    {
        Assert.Throws<NapriseInvalidUrlException>(() =>
        {
            var service = new Discord(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        });
    }

    [Fact]
    public async void TestRequestPayload()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var client = handler.CreateClient();

        handler.SetupRequest(HttpMethod.Post, "https://discord.com/api/webhooks/1234567890/abcdefghijklmnopqrstuvwxyz", async r =>
        {
            Assert.NotNull(r.Content);
            var reqBody = await r.Content.ReadAsStringAsync();
            const string expected = @"{""avatar_url"":""https://example.com/avatar.png"",""username"":""TestUser"",""tts"":true,""embeds"":[{""title"":""From Naprise"",""description"":""_Hello_ **World**! :heart:"",""color"":3842871}]}";
            Assert.Equal(expected, reqBody);
            return true;
        }).ReturnsResponse(HttpStatusCode.OK);

        var url = "discord://1234567890/abcdefghijklmnopqrstuvwxyz?username=TestUser&avatar_url=https://example.com/avatar.png&tts";
        var service = new Discord(new ServiceConfig(new Url(url), new NapriseAsset(), () => client));

        await service.NotifyAsync(new Message
        {
            Markdown = @"_Hello_ **World**! :heart:",
            Title = "From Naprise",
            Type = MessageType.Success,
        });

        handler.VerifyAll();
    }
}
