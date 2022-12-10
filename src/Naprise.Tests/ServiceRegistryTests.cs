namespace Naprise.Tests;
public class ServiceRegistryTests
{
    public ServiceRegistryTests()
    {
        this.ServiceRegistry = new ServiceRegistry().AddDefaultServices().Add<MockService>();
    }

    public ServiceRegistry ServiceRegistry { get; }

    [Fact]
    public void EnsureCreateMethodsWillNotInfiniteLoop()
    {
        Naprise.DefaultRegistry = this.ServiceRegistry;

        // ServiceRegistryExtensions.Create()
        // params string[] urls
        this.ServiceRegistry.Create(urls: "mock://").Should().BeOfType<MockService>();
        this.ServiceRegistry.Create("mock://", "mock://").Should().BeOfType<CompositeNotifier>();
        this.ServiceRegistry.Create(urls: new[] { "mock://" }).Should().BeOfType<MockService>();
        this.ServiceRegistry.Create(urls: new[] { "mock://", "mock://" }).Should().BeOfType<CompositeNotifier>();

        // params Url[] urls
        this.ServiceRegistry.Create(urls: new Url("mock://")).Should().BeOfType<MockService>();
        this.ServiceRegistry.Create(new Url("mock://"), new Url("mock://")).Should().BeOfType<CompositeNotifier>();
        this.ServiceRegistry.Create(urls: new[] { new Url("mock://") }).Should().BeOfType<MockService>();
        this.ServiceRegistry.Create(urls: new[] { new Url("mock://"), new Url("mock://") }).Should().BeOfType<CompositeNotifier>();

        // ServiceRegistry.Create()
        // IEnumerable<Url> serviceUrls
        this.ServiceRegistry.Create(serviceUrls: new[] { new Url("mock://") }).Should().BeOfType<MockService>();
        this.ServiceRegistry.Create(serviceUrls: new[] { new Url("mock://"), new Url("mock://") }).Should().BeOfType<CompositeNotifier>();

        // Naprise.Create()
        // params string[] urls
        Naprise.Create(urls: "mock://").Should().BeOfType<MockService>();
        Naprise.Create("mock://", "mock://").Should().BeOfType<CompositeNotifier>();
        Naprise.Create(urls: new[] { "mock://" }).Should().BeOfType<MockService>();
        Naprise.Create(urls: new[] { "mock://", "mock://" }).Should().BeOfType<CompositeNotifier>();

        // params Url[] urls
        Naprise.Create(urls: new Url("mock://")).Should().BeOfType<MockService>();
        Naprise.Create(new Url("mock://"), new Url("mock://")).Should().BeOfType<CompositeNotifier>();
        Naprise.Create(urls: new[] { new Url("mock://") }).Should().BeOfType<MockService>();
        Naprise.Create(urls: new[] { new Url("mock://"), new Url("mock://") }).Should().BeOfType<CompositeNotifier>();

        // IEnumerable<Url> urls
        Naprise.Create(urls: new List<Url> { new Url("mock://") }).Should().BeOfType<MockService>();
        Naprise.Create(urls: new List<Url> { new Url("mock://"), new Url("mock://") }).Should().BeOfType<CompositeNotifier>();
    }

    [Fact]
    public void EnsureUsingCorrectHttpClientWhenNull()
    {
        this.ServiceRegistry.HttpClient = null;
        var notifier = this.ServiceRegistry.Create(urls: "naprise-a://");
        notifier.Should()
                    .BeOfType<MockService>()
                    .Which.Config.HttpClientFactory()
                    .Should()
                    .BeSameAs(Naprise.DefaultHttpClient);
    }

    [Fact]
    public void EnsureUsingCorrectHttpClientWhenSet()
    {
        this.ServiceRegistry.HttpClient = new HttpClient();
        var notifier = this.ServiceRegistry.Create(urls: "naprise-a://");
        notifier.Should()
                    .BeOfType<MockService>()
                    .Which.Config.HttpClientFactory()
                    .Should()
                    .NotBeSameAs(Naprise.DefaultHttpClient).And
                    .BeSameAs(this.ServiceRegistry.HttpClient);
    }

    [Fact]
    public void EnsureUsingCorrectHttpClientAfterCreated()
    {
        var notifier = this.ServiceRegistry.Create(urls: "naprise-a://").Should().BeOfType<MockService>().Subject;

        notifier.Config.HttpClientFactory().Should().BeSameAs(Naprise.DefaultHttpClient);

        var httpclient1 = new HttpClient();
        var httpclient2 = new HttpClient();

        this.ServiceRegistry.HttpClient = httpclient1;
        notifier.Config.HttpClientFactory()
            .Should()
            .NotBeSameAs(Naprise.DefaultHttpClient).And
            .BeSameAs(httpclient1);

        this.ServiceRegistry.HttpClient = httpclient2;
        notifier.Config.HttpClientFactory()
            .Should()
            .NotBeSameAs(Naprise.DefaultHttpClient).And
            .NotBeSameAs(httpclient1).And
            .BeSameAs(httpclient2);
    }

    [Theory]
    [InlineData("naprise-a://host")]
    [InlineData("naprise-b://host")]
    [InlineData("mock://host")]
    public void EnsureCorrectServiceSelected(string url)
    {
        var notifier = this.ServiceRegistry.Create(url);
        notifier.Should().BeOfType<MockService>().Which.Config.Url.ToString().Should().Be(url);
    }

    [Theory]
    [InlineData("unknown://host")]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    public void EnsureThrowOnUnknownScheme(string url)
    {
        this.ServiceRegistry.IgnoreUnknownScheme = false;
        Assert.Throws<NapriseUnknownSchemeException>(() => this.ServiceRegistry.Create(url));
    }

    [Theory]
    [InlineData("unknown://host")]
    [InlineData("http://example.com")]
    [InlineData("https://example.com")]
    public void EnsureNoopNotifierOnUnknownScheme(string url)
    {
        this.ServiceRegistry.IgnoreUnknownScheme = true;
        this.ServiceRegistry.Create(url).Should().BeSameAs(Naprise.NoopNotifier);
    }

    [Fact]
    public void TestSingleInvalidUrlWithIgnoreFalse()
    {
        this.ServiceRegistry.IgnoreInvalidUrl = false;
        Assert.Throws<NapriseInvalidUrlException>(() => this.ServiceRegistry.Create("mock://throw"));
    }

    [Fact]
    public void TestSingleInvalidUrlWithIgnoreTrue()
    {
        this.ServiceRegistry.IgnoreInvalidUrl = true;
        this.ServiceRegistry.Create("mock://throw").Should().BeSameAs(Naprise.NoopNotifier);
    }

    [Fact]
    public void TestMultipleInvalidUrlWithIgnoreFalse()
    {
        this.ServiceRegistry.IgnoreInvalidUrl = false;
        Assert.Throws<NapriseInvalidUrlException>(() => this.ServiceRegistry.Create("mock://throw", "mock://throw"));
    }

    [Fact]
    public void TestMultipleInvalidUrlWithIgnoreTrue()
    {
        this.ServiceRegistry.IgnoreInvalidUrl = true;
        this.ServiceRegistry.Create("mock://throw", "mock://throw").Should().BeSameAs(Naprise.NoopNotifier);
    }

    public static IEnumerable<object[]> GetMultilineSchemeTestData() => throw new NotImplementedException(); // TODO: add multi-service notifer test

    [NapriseNotificationService("MockService", "mock", "naprise-a", "naprise-b")]
    private class MockService : NotificationService
    {
        public ServiceConfig Config { get; }

        public MockService(ServiceConfig config) : base(config)
        {
            this.Config = config;

            if (config.Url.Host == "throw")
                throw new NapriseInvalidUrlException("Invalid URL");
        }

        public override Task NotifyAsync(Message message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
