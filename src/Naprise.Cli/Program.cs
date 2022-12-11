using System.CommandLine;

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

    private static RootCommand BuildCommand()
    {
        var c = new RootCommand(description: "Naprise CLI");

        c.AddCommand(List.command);
        c.AddCommand(Send.command);

        return c;
    }
}
