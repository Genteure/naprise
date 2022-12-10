using Naprise.Service;

namespace Naprise.Tests.Service;
public class PushDeerTests
{
    [Theory]
    [InlineData("pushdeers://api2.pushdeer.com/x6oJU9DE7wWr5nW", true, "api2.pushdeer.com", "", "x6oJU9DE7wWr5nW")]
    [InlineData("pushdeers://user:pass@api2.pushdeer.com/x6oJU9DE7wWr5nW", true, "api2.pushdeer.com", "user:pass", "x6oJU9DE7wWr5nW")]
    [InlineData("pushdeer://localhost:8080/x6oJU9DE7wWr5nW", false, "localhost:8080", "", "x6oJU9DE7wWr5nW")]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string userinfo, string pushkey)
    {
        var service = new PushDeer(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(userinfo, service.userinfo);
        Assert.Equal(pushkey, service.pushkey);
    }
}
