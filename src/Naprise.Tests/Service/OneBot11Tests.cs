using Naprise.Service;

namespace Naprise.Tests.Service;
public class OneBot11Tests
{
    [Theory]
    [InlineData("onebot11://mytoken@localhost:8080/private/123456", false, "localhost:8080", "mytoken", "private", "123456", null)]
    [InlineData("onebot11://mytoken@localhost:8080/group/123456", false, "localhost:8080", "mytoken", "group", null, "123456")]
    [InlineData("onebot11s://mytoken@localhost:8080/private/123456", true, "localhost:8080", "mytoken", "private", "123456", null)]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string access_token, string message_type, string? user_id, string? group_id)
    {
        var service = new OneBot11(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(access_token, service.access_token);
        Assert.Equal(message_type, service.message_type);
        Assert.Equal(user_id, service.user_id);
        Assert.Equal(group_id, service.group_id);
    }

    [Theory]
    [InlineData("onebot11://localhost")] // missing detail_type
    [InlineData("onebot11://localhost/aaaaaa")] // invalid message_type
    [InlineData("onebot11://localhost/private")] // missing user_id
    [InlineData("onebot11://localhost/group")] // missing group_id
    public void ExpectNapriseInvalidUrlException(string url)
    {
        Assert.Throws<NapriseInvalidUrlException>(() =>
        {
            var service = new OneBot11(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        });
    }
}
