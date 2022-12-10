using Naprise.Service.MailKit;

namespace Naprise.Tests.Service;

public class MailKitTests
{
    [Theory]
    [InlineData("email://user:pass@gmail.com", true, "smtp.gmail.com", 465, "user@gmail.com", "pass", "user@gmail.com", "user@gmail.com")]
    [InlineData("email://12345:pAs$w0r*@qq.com", true, "smtp.qq.com", 465, "12345@qq.com", "pAs$w0r*", "12345@qq.com", "12345@qq.com")]
    [InlineData("smtp://example.com:25/user/pass/a@example.com/b@example.com", false, "example.com", 25, "user", "pass", "a@example.com", "b@example.com")]
    [InlineData("smtps://example.com:465/user/pass/a@example.com/b@example.com", true, "example.com", 465, "user", "pass", "a@example.com", "b@example.com")]
    [InlineData("smtps://example.com:465/a@example.com/b@example.com", true, "example.com", 465, null, null, "a@example.com", "b@example.com")]
    [InlineData("smtps://example.com:465/user@example.org/pAs$w0r*/a@example.com/b@example.com", true, "example.com", 465, "user@example.org", "pAs$w0r*", "a@example.com", "b@example.com")]
    public void TestUrlParsing(string url, bool useSsl, string host, int port, string? username, string? password, string from, string to)
    {
        var service = new MailKitEmail(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(useSsl, service.useSsl);
        Assert.Equal(host, service.host);
        Assert.Equal(port, service.port);
        Assert.Equal(username, service.username);
        Assert.Equal(password, service.password);
        Assert.Equal(from, service.from);
        Assert.Equal(to, service.to);
    }

    [Theory]
    [InlineData("email://example.com")] // domain not supported
    [InlineData("email://user@example.com")]
    [InlineData("email://user:pass@example.com")]
    [InlineData("email://gmail.com")] // missing username and password
    [InlineData("email://user@gmail.com")] // missing password
    [InlineData("smtp://example.com")] // missing port
    [InlineData("smtp://example.com/from/to")] // missing port
    [InlineData("smtp://example.com/user/pass/from/to")] // missing port
    [InlineData("smtps://example.com")] // missing port
    [InlineData("smtps://example.com/from/to")] // missing port
    [InlineData("smtps://example.com/user/pass/from/to")] // missing port
    [InlineData("smtp://example.com:25")] // missing path arguments
    [InlineData("smtp://example.com:25/from")] // missing path arguments
    [InlineData("smtp://example.com:25/user/pass/from")] // missing path arguments
    [InlineData("smtp://example.com:25/user/pass/from/to/what")] // too much arguments
    public void TestInvalidUrl(string url)
    {
        Assert.Throws<NapriseInvalidUrlException>(() =>
        {
            var service = new MailKitEmail(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        });
    }
}
