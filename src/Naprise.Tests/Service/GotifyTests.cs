using Naprise.Service;

namespace Naprise.Tests.Service;
public class GotifyTests
{
    [Theory]
    [InlineData("gotify://example.com/1234567890", false, "example.com", "1234567890", null, null)]
    [InlineData("gotifys://example.com/1234567890", true, "example.com", "1234567890", null, null)]
    [InlineData("gotify://127.0.0.1:9980/Axjd7nAIBIi0iKv?priority=4&click_url=https://example.com", false, "127.0.0.1:9980", "Axjd7nAIBIi0iKv", 4, "https://example.com")]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string token, int? priority, string? clickUrl)
    {
        var service = new Gotify(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(token, service.token);
        Assert.Equal(priority, service.priority);
        Assert.Equal(clickUrl, service.clickUrl);
    }
}
