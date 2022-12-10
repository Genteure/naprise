using Naprise.Service;

namespace Naprise.Tests.Service;
public class OneBot12Tests
{
    [Theory]
    [InlineData("onebot12://mytoken@localhost:8080/private/123456", false, "localhost:8080", "mytoken", "private", "123456", null, null, null)]
    [InlineData("onebot12://mytoken@localhost:8080/group/123456", false, "localhost:8080", "mytoken", "group", null, "123456", null, null)]
    [InlineData("onebot12://mytoken@localhost:8080/channel/123456/654987", false, "localhost:8080", "mytoken", "channel", null, null, "123456", "654987")]
    [InlineData("onebot12s://mytoken@localhost:8080/private/123456", true, "localhost:8080", "mytoken", "private", "123456", null, null, null)]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string access_token, string detail_type, string? user_id, string? group_id, string? guild_id, string? channel_id)
    {
        var service = new OneBot12(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));

        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(access_token, service.access_token);
        Assert.Equal(detail_type, service.detail_type);
        Assert.Equal(user_id, service.user_id);
        Assert.Equal(group_id, service.group_id);
        Assert.Equal(guild_id, service.guild_id);
        Assert.Equal(channel_id, service.channel_id);
    }

    [Theory]
    [InlineData("onebot12://localhost")] // missing detail_type
    [InlineData("onebot12://localhost/aaaaaa")] // invalid detail_type
    [InlineData("onebot12://localhost/private")] // missing user_id
    [InlineData("onebot12://localhost/group")] // missing group_id
    [InlineData("onebot12://localhost/channel")] // missing guild_id and channel_id
    [InlineData("onebot12://localhost/channel?guild_id=123456")] // missing channel_id
    [InlineData("onebot12://localhost/channel?channel_id=654987")] // missing guild_id
    public void ExpectNapriseInvalidUrlException(string url)
    {
        Assert.Throws<NapriseInvalidUrlException>(() =>
        {
            var service = new OneBot12(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        });
    }
}
