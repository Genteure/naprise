namespace Naprise.DocGenerator;

internal class Program
{
    private static void Main(string[] args)
    {
        var basePath = FindBasePath(args);

        if (basePath is null)
        {
            Console.WriteLine("Base path not found");
            Environment.Exit(1);
        }

        Console.WriteLine($"Running at path: {basePath}");

        var generator = new Generator(basePath);
        generator.Generate();
    }

    private static string? FindBasePath(string[] args)
    {
        static bool ValidPath(string? path)
        {
            if (!Directory.Exists(path))
                return false;

            if (!File.Exists(Path.Combine(path, "README.md")))
                return false;

            if (!Directory.Exists(Path.Combine(path, "src", "Naprise")))
                return false;

            if (!Directory.Exists(Path.Combine(path, "docs")))
                return false;

            return true;
        }

        if (args.Length > 0)
        {
            var path = args[0];
            return ValidPath(path) ? path : null;
        }
        else
        {
            var path = new DirectoryInfo(Environment.ProcessPath!);
            while (path != null)
            {
                if (ValidPath(path.FullName))
                    return path.FullName;
                path = path.Parent;
            }

            return null;
        }
    }
}
