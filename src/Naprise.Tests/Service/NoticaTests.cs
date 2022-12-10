using Naprise.Service;

namespace Naprise.Tests.Service;
public class NoticaTests
{
    [Theory]
    [InlineData("noticas://notica.us/Ht4d1H", true, "notica.us", "", "Ht4d1H")]
    [InlineData("notica://localhost:1234/AbC123", false, "localhost:1234", "", "AbC123")]
    [InlineData("noticas://localhost:1234/AbC123", true, "localhost:1234", "", "AbC123")]
    [InlineData("notica://user:pass@localhost:1234/AbC123", false, "localhost:1234", "user:pass", "AbC123")]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string userinfo, string token)
    {
        var service = new Notica(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(userinfo, service.userinfo);
        Assert.Equal(token, service.token);
    }
}
