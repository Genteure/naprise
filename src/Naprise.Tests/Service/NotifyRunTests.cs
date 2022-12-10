using Naprise.Service;

namespace Naprise.Tests.Service;
public class NotifyRunTests
{
    [Theory]
    [InlineData("notifyruns://notify.run/wiFAz0Kp2BsDicG1zafTk", true, "notify.run", "", "wiFAz0Kp2BsDicG1zafTk")]
    [InlineData("notifyrun://localhost:1234/wiFAz0Kp2BsDicG1zafTk", false, "localhost:1234", "", "wiFAz0Kp2BsDicG1zafTk")]
    [InlineData("notifyruns://localhost:1234/wiFAz0Kp2BsDicG1zafTk", true, "localhost:1234", "", "wiFAz0Kp2BsDicG1zafTk")]
    [InlineData("notifyrun://user:pass@localhost:1234/wiFAz0Kp2BsDicG1zafTk", false, "localhost:1234", "user:pass", "wiFAz0Kp2BsDicG1zafTk")]
    public void TestUrlParsing(string url, bool https, string hostAndPort, string userinfo, string channel)
    {
        var service = new NotifyRun(new ServiceConfig(new Url(url), new NapriseAsset(), () => new HttpClient()));
        Assert.Equal(https, service.https);
        Assert.Equal(hostAndPort, service.hostAndPort);
        Assert.Equal(userinfo, service.userinfo);
        Assert.Equal(channel, service.channel);
    }
}
