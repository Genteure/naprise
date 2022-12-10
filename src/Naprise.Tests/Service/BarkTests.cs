using Naprise.Service;

namespace Naprise.Tests.Service;
public class BarkTests
{
    [Theory]
    [InlineData("barks://api.day.app/YXeWgbg8grJbPdVZ", true, "api.day.app", "YXeWgbg8grJbPdVZ", null, null, null, null, null)]
    [InlineData("bark://localhost:8080/YXeWgbg8grJbPdVZ", false, "localhost:8080", "YXeWgbg8grJbPdVZ", null, null, null, null, null)]
    [InlineData("barks://api.day.app/YXeWgbg8grJbPdVZ?url=https://example.com", true, "api.day.app", "YXeWgbg8grJbPdVZ", "https://example.com", null, null, null, null)]
    [InlineData("barks://api.day.app/YXeWgbg8grJbPdVZ?group=testgroup", true, "api.day.app", "YXeWgbg8grJbPdVZ", null, "testgroup", null, null, null)]
    [InlineData("barks://api.day.app/YXeWgbg8grJbPdVZ?icon=https://example.com/icon.svg", true, "api.day.app", "YXeWgbg8grJbPdVZ", null, null, "https://example.com/icon.svg", null, null)]
    [InlineData("barks://api.day.app/YXeWgbg8grJbPdVZ?level=timeSensitive", true, "api.day.app", "YXeWgbg8grJbPdVZ", null, null, null, "timeSensitive", null)]
    [InlineData("barks://api.day.app/YXeWgbg8grJbPdVZ?sound=alarm.caf", true, "api.day.app", "YXeWgbg8grJbPdVZ", null, null, null, null, "alarm.caf")]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string token, string? click_url, string? group, string? icon, string? level, string? sound)
    {
        var service = new Bark(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(token, service.token);
        Assert.Equal(click_url, service.click_url);
        Assert.Equal(group, service.group);
        Assert.Equal(icon, service.icon);
        Assert.Equal(level, service.level);
        Assert.Equal(sound, service.sound);
    }
}
