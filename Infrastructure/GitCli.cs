namespace Dostar.Cli;

internal static class GitCli
{
    private const string GitDirectoryName = ".git";

    internal static async Task CloneAsync(string repoUrl, string outputDir)
    {
        var exitCode = await ProcessRunner.RunAsync("git", ["clone", repoUrl, outputDir], Directory.GetCurrentDirectory());
        if (exitCode != 0)
            throw new InvalidOperationException("git clone failed.");
    }

    internal static void RemoveHistory(string outputDir)
    {
        var gitDir = Path.Combine(outputDir, GitDirectoryName);
        if (Directory.Exists(gitDir))
            DeleteDirectoryForce(gitDir);
    }

    private static void DeleteDirectoryForce(string path)
    {
        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            var attrs = File.GetAttributes(file);
            if ((attrs & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(file, attrs & ~FileAttributes.ReadOnly);
        }

        Directory.Delete(path, recursive: true);
    }
}
