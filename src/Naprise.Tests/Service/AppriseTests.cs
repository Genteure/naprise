using Naprise.Service;

namespace Naprise.Tests.Service;
public class AppriseTests
{
    [Theory]
    [InlineData("apprise://localhost/thisistoken", false, "", "localhost", "thisistoken", Format.Unknown, null)]
    [InlineData("apprises://localhost/thisistoken", true, "", "localhost", "thisistoken", Format.Unknown, null)]
    [InlineData("apprise://localhost:8080/thisistoken", false, "", "localhost:8080", "thisistoken", Format.Unknown, null)]
    [InlineData("apprises://localhost:8080/thisistoken", true, "", "localhost:8080", "thisistoken", Format.Unknown, null)]
    [InlineData("apprise://username:password@localhost/thisistoken", false, "username:password", "localhost", "thisistoken", Format.Unknown, null)]
    [InlineData("apprise://localhost/thisistoken?tag=test", false, "", "localhost", "thisistoken", Format.Unknown, "test")]
    [InlineData("apprise://localhost/thisistoken?format=text", false, "", "localhost", "thisistoken", Format.Text, null)]
    [InlineData("apprise://localhost/thisistoken?format=markdown", false, "", "localhost", "thisistoken", Format.Markdown, null)]
    [InlineData("apprise://localhost/thisistoken?format=html", false, "", "localhost", "thisistoken", Format.Html, null)]
    public void TestUrlParsing(string url, bool https, string userinfo, string hostAndPort, string token, Format format, string? tag)
    {
        var service = new Apprise(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(https, service.https);
        Assert.Equal(userinfo, service.userinfo);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(token, service.token);
        Assert.Equal(format, service.format);
        Assert.Equal(tag, service.tag);
    }
}
