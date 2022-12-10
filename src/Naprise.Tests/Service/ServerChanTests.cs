using Naprise.Service;

namespace Naprise.Tests.Service;
public class ServerChanTests
{
    [Theory]
    [InlineData("serverchan://SCT123456@serverchan", "SCT123456", null, null)]
    [InlineData("serverchan://6AbCd4321@serverchan", "6AbCd4321", null, null)]
    [InlineData("serverchan://SCT123456@serverchan?channel=1|2", "SCT123456", "1|2", null)]
    [InlineData("serverchan://SCT123456@serverchan?openid=aaaaa", "SCT123456", null, "aaaaa")]
    [InlineData("serverchan://SCT123456@serverchan?openid=aaaaa,bbbbb", "SCT123456", null, "aaaaa,bbbbb")]
    public void TestUrlParsing(string url, string token, string? channel, string? openid)
    {
        var service = new ServerChan(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(token, service.token);
        Assert.Equal(channel, service.channel);
        Assert.Equal(openid, service.openid);
    }
}
