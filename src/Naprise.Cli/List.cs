using Naprise.Service.MailKit;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace Naprise.Cli;
internal class List
{
    public static readonly Command command = new(name: "list", description: "")
    {
    };

    static List()
    {
        command.SetHandler(Handle);
    }

    private static void Handle(InvocationContext invocationContext)
    {
        // do we want to provide a API to list all services in a registry?

        var list = new List<Type>(ServiceRegistry.DefaultServices)
        {
            typeof(MailKitEmail)
        };
        list.Sort((a, b) => a.Name.CompareTo(b.Name));

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Name")
            .AddColumn("Schemes");

        var first = true;
        foreach (var service in list)
        {
            if (!first)
            {
                table.AddEmptyRow();
            }
            first = false;
            table.AddRow(service.Name.EscapeMarkup(), string.Join("\n", service.GetCustomAttribute<NapriseNotificationServiceAttribute>()!.Schemes.Select(x => x + "://")).EscapeMarkup());
        }

        AnsiConsole.Write(table);
    }
}
