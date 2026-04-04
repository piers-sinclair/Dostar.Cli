namespace Dostar.Cli;

internal static class CsprojReader
{
    internal static async Task<string> ReadTargetFrameworkAsync(string csprojPath)
    {
        if (!File.Exists(csprojPath))
        {
            Console.Error.WriteLine($"Warning: could not find {csprojPath} to detect target framework; defaulting to net10.0.");
            return "net10.0";
        }

        var xml = XDocument.Parse(await File.ReadAllTextAsync(csprojPath));
        var value = xml.Descendants("TargetFramework").FirstOrDefault()?.Value;
        if (value is null)
        {
            Console.Error.WriteLine($"Warning: <TargetFramework> not found in {csprojPath}; defaulting to net10.0.");
            return "net10.0";
        }

        return value;
    }
}
