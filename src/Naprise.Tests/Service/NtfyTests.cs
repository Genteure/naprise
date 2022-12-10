using Naprise.Service;

namespace Naprise.Tests.Service;
public class NtfyTests
{
    [Theory]
    [InlineData("ntfy://localhost/my_ntfy_topic", false, "localhost", "", "my_ntfy_topic", new string[] { }, null, null, null, null)]
    [InlineData("ntfys://localhost/my_ntfy_topic", true, "localhost", "", "my_ntfy_topic", new string[] { }, null, null, null, null)]
    [InlineData("ntfy://localhost:8080/my_ntfy_topic", false, "localhost:8080", "", "my_ntfy_topic", new string[] { }, null, null, null, null)]
    [InlineData("ntfys://localhost:8080/my_ntfy_topic", true, "localhost:8080", "", "my_ntfy_topic", new string[] { }, null, null, null, null)]
    [InlineData("ntfy://username:password@localhost/my_ntfy_topic", false, "localhost", "username:password", "my_ntfy_topic", new string[] { }, null, null, null, null)]
    [InlineData("ntfy://localhost/my_ntfy_topic?tags=test", false, "localhost", "", "my_ntfy_topic", new string[] { "test" }, null, null, null, null)]
    [InlineData("ntfy://localhost/my_ntfy_topic?tags=a,b,c&tags=d,e&tags=f", false, "localhost", "", "my_ntfy_topic", new string[] { "a", "b", "c", "d", "e", "f" }, null, null, null, null)]
    [InlineData("ntfy://localhost/my_ntfy_topic?priority=1", false, "localhost", "", "my_ntfy_topic", new string[] { }, 1, null, null, null)]
    [InlineData("ntfy://localhost/my_ntfy_topic?click=https://example.com", false, "localhost", "", "my_ntfy_topic", new string[] { }, null, "https://example.com", null, null)]
    [InlineData("ntfy://localhost/my_ntfy_topic?delay=1m", false, "localhost", "", "my_ntfy_topic", new string[] { }, null, null, "1m", null)]
    [InlineData("ntfy://localhost/my_ntfy_topic?email=user@example.com", false, "localhost", "", "my_ntfy_topic", new string[] { }, null, null, null, "user@example.com")]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string userinfo, string topic, string[] tags, int? priority, string? click, string? delay, string? email)
    {
        var service = new Ntfy(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(userinfo, service.userinfo);
        Assert.Equal(topic, service.topic);
        Assert.Equal(tags, service.tags);
        Assert.Equal(priority, service.priority);
        Assert.Equal(click, service.click);
        Assert.Equal(delay, service.delay);
        Assert.Equal(email, service.email);
    }
}
