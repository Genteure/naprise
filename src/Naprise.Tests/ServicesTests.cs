using Naprise.Service;
using Naprise.Service.MailKit;
using System.Reflection;

namespace Naprise.Tests;
public class ServicesTests
{
    public static readonly IReadOnlyList<Assembly> ServiceAssemblies = new List<Assembly>
    {
        typeof(MailKitEmail).Assembly,
    };

    [Fact]
    public void EnsureMainProjServicesAreValid()
    {
        var types = typeof(ServiceRegistry).Assembly.GetTypes()
            .Where(t => t.Namespace == "Naprise.Service" && t.IsPublic)
            .ToArray();

        ServiceRegistry.DefaultServices.Should().BeEquivalentTo(types);
    }

    [Fact]
    public void EnsureMainProjNoNonPublicClasses()
    {
        var types = typeof(ServiceRegistry).Assembly.GetTypes()
            .Where(t => t.Namespace == "Naprise.Service" && !t.IsNested && !t.IsPublic)
            .ToArray();

        types.Should().BeEquivalentTo(new[] { typeof(Template) }); // only the template is allowed
    }

    [Fact]
    public void EnsureMainProjAllNestClassAreNotPublic()
    {
        var types = typeof(ServiceRegistry).Assembly.GetTypes()
            .Where(t => t.Namespace == "Naprise.Service" && t.IsNestedPublic)
            .ToArray();

        types.Should().BeEmpty();
    }

    [Fact]
    public void EnsureBuiltinServiceDontHaveDuplicateSchemes()
    {
        var types = ServiceAssemblies.SelectMany(x => x.GetTypes().Where(x => x.IsPublic && typeof(INotifier).IsAssignableFrom(x))).ToList();
        types.AddRange(ServiceRegistry.DefaultServices);

        types.SelectMany(x => x.GetCustomAttribute<NapriseNotificationServiceAttribute>() is { } attr ? attr.Schemes : throw new InvalidOperationException($"Service {x} is missing the {nameof(NapriseNotificationServiceAttribute)} attribute."))
            .Should()
            .OnlyHaveUniqueItems();
    }

    [Fact]
    public void EnsureMainProjServiceHaveTests()
    {
        var a = typeof(ServicesTests).Assembly;

        var tests = ServiceRegistry.DefaultServices
            .Select(x => $"Naprise.Tests.Service.{x.Name}Tests")
            .Select(x => a.GetType(x))
            .ToArray();

        if (tests.Contains(null))
        {
            // find what's missing for better error messages
            var pnames = ServiceRegistry.DefaultServices.Select(x => x.Name).ToList();
            var tnames = tests.Where(x => x is not null).Select(x => x!.Name.Substring(0, x.Name.Length - 5)).ToArray();
            foreach (var n in tnames)
            {
                Assert.True(pnames.Remove(n), $"Should able to remove {n} from the list of service names.");
            }

            pnames.Should().BeEmpty();
        }
    }
}
