using Naprise.Service;

namespace Naprise.Tests.Service;
public class PushPlusTests
{
    [Theory]
    [InlineData("pushplus://abcdefg@wechat/", "abcdefg", "wechat")]
    [InlineData("pushplus://a123335@cp", "a123335", "cp")]
    public void TestUrlParsing(string url, string token, string channel)
    {
        var service = new PushPlus(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(token, service.token);
        Assert.Equal(channel, service.channel);
    }
}
