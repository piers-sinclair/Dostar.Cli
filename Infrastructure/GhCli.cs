namespace Dostar.Cli;

internal static class GhCli
{
    internal static bool IsAvailable()
    {
        try
        {
            var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            var exeNames = OperatingSystem.IsWindows()
                ? new[] { "gh.exe", "gh.cmd", "gh.bat" }
                : new[] { "gh" };

            return path
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(dir => exeNames.Select(exe => Path.Combine(dir, exe)))
                .Any(File.Exists);
        }
        catch
        {
            return false;
        }
    }
}
