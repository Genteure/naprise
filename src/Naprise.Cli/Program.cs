using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;

namespace Naprise.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        if (Console.IsOutputRedirected || Console.IsErrorRedirected || Console.IsInputRedirected)
        {
            Console.Error.WriteLine("""
                WARNING: naprisecli is designed mainly for interactive use and as a way to test Naprise.
                It does not have have a stable CLI interface, use in scripts at your own risk.
                Consider using Apprise instead: https://github.com/caronc/apprise

                """);
        }

        Naprise.DefaultRegistry.AddMailKit();

        await BuildCommand().InvokeAsync(args);
    }

    private static readonly Option<string?> title = new(aliases: new[] { "-t", "--title" }, description: "Title of the message");
    private static readonly Option<string?> text = new(aliases: new[] { "-e", "--text" }, description: "Body of the message in plain text");
    private static readonly Option<string?> markdown = new(aliases: new[] { "-m", "--markdown" }, description: "Body of the message in Markdown");
    private static readonly Option<string?> html = new(aliases: new[] { "-l", "--html" }, description: "Body of the message in HTML");
    private static readonly Option<bool> verbose = new(aliases: new[] { "-v", "--verbose" }, description: "Show verbose output");
    private static readonly Option<TimeSpan> timeout = new(aliases: new[] { "--timeout" }, getDefaultValue: () => TimeSpan.FromSeconds(10), description: "Set timeout for HTTP requests");
    private static readonly Option<string?> proxy = new(aliases: new[] { "-x", "--proxy" }, description: "Proxy to use for HTTP requests");
    private static readonly Option<bool> insecure = new(aliases: new[] { "-k", "--insecure" }, getDefaultValue: () => false, description: "Allow insecure server connections when using SSL");
    private static readonly Argument<string[]> urls = new(name: "urls", description: "URLs to send the message to");

    private static RootCommand BuildCommand()
    {
        var c = new RootCommand(description: "Naprise CLI");

        c.AddOption(title);
        c.AddOption(text);
        c.AddOption(markdown);
        c.AddOption(html);
        c.AddOption(verbose);
        c.AddOption(timeout);
        c.AddOption(proxy);
        c.AddOption(insecure);

        c.AddArgument(urls);

        c.SetHandler(SendAsync);

        c.AddValidator(v =>
        {
            if (v.GetValueForArgument(urls).Length == 0)
                v.ErrorMessage = "At least one URL must be specified";
        });

        return c;
    }

    private static async Task SendAsync(InvocationContext invocationContext)
    {
        // read args

        var result = invocationContext.ParseResult;

        var title = result.GetValueForOption(Program.title);
        var text = result.GetValueForOption(Program.text);
        var markdown = result.GetValueForOption(Program.markdown);
        var html = result.GetValueForOption(Program.html);
        var verbose = result.GetValueForOption(Program.verbose);
        var timeout = result.GetValueForOption(Program.timeout);
        var proxy = result.GetValueForOption(Program.proxy);
        var insecure = result.GetValueForOption(Program.insecure);

        var urls = result.GetValueForArgument(Program.urls);

        // setup HttpClient

        var handler = new HttpClientHandler();

        if (proxy is not null)
            handler.Proxy = new WebProxy(Address: proxy, BypassOnLocal: true);

        if (insecure)
            handler.ServerCertificateCustomValidationCallback = (req, cert, chain, error) => true;

        var client = new HttpClient(handler)
        {
            Timeout = timeout
        };
        Naprise.DefaultHttpClient = client;

        // send message

        var message = new Message
        {
            Title = title,
            Text = text,
            Markdown = markdown,
            Html = html,
        };

        var notifier = Naprise.Create(urls);

        try
        {
            await notifier.NotifyAsync(message);
            Console.WriteLine("Message sent successfully");
        }
        catch (Exception)
        {
            // TODO: pretty print NapriseNotifyFailedException, AggregateException and it's inner exceptions
            throw;
        }
    }
}
