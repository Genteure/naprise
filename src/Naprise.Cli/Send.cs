using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;

namespace Naprise.Cli;
internal static class Send
{
    private static readonly Option<string?> title = new(aliases: new[] { "-t", "--title" }, description: "Title of the message");
    private static readonly Option<MessageType> type = new(aliases: new[] { "-y", "--type" }, description: "Type of the message");
    private static readonly Option<string?> text = new(aliases: new[] { "-e", "--text" }, description: "Body of the message in plain text");
    private static readonly Option<string?> markdown = new(aliases: new[] { "-m", "--markdown" }, description: "Body of the message in Markdown");
    private static readonly Option<string?> html = new(aliases: new[] { "-l", "--html" }, description: "Body of the message in HTML");
    private static readonly Option<bool> verbose = new(aliases: new[] { "-v", "--verbose" }, description: "Show verbose output");
    private static readonly Option<TimeSpan> timeout = new(aliases: new[] { "--timeout" }, getDefaultValue: () => TimeSpan.FromSeconds(10), description: "Set timeout for HTTP requests");
    private static readonly Option<string?> proxy = new(aliases: new[] { "-x", "--proxy" }, description: "Proxy to use for HTTP requests");
    private static readonly Option<bool> insecure = new(aliases: new[] { "-k", "--insecure" }, getDefaultValue: () => false, description: "Allow insecure server connections when using SSL");
    private static readonly Argument<string[]> urls = new(name: "urls", description: "URLs to send the message to");

    public static readonly Command command = new(name: "send", description: "Send a message to one or more URLs")
    {
        title,
        type,
        text,
        markdown,
        html,
        verbose,
        timeout,
        proxy,
        insecure,
        urls
    };

    static Send()
    {
        command.SetHandler(SendAsync);

        command.AddValidator(v =>
        {
            if (v.GetValueForArgument(urls).Length == 0)
                v.ErrorMessage = "At least one URL must be specified";
        });
    }

    private static async Task SendAsync(InvocationContext invocationContext)
    {
        // read args

        var result = invocationContext.ParseResult;

        var title = result.GetValueForOption(Send.title);
        var type = result.GetValueForOption(Send.type);
        var text = result.GetValueForOption(Send.text);
        var markdown = result.GetValueForOption(Send.markdown);
        var html = result.GetValueForOption(Send.html);
        var verbose = result.GetValueForOption(Send.verbose);
        var timeout = result.GetValueForOption(Send.timeout);
        var proxy = result.GetValueForOption(Send.proxy);
        var insecure = result.GetValueForOption(Send.insecure);

        var urls = result.GetValueForArgument(Send.urls);

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
            Type = type,
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
