namespace Dostar.Cli;

internal sealed class RemoveFeatureService(string name, bool dryRun, bool yes, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string NameKebab => name.ToKebabCase();
    private string FeatureDir => Path.Combine(_root.Root, "frontend", "src", "features", NameKebab);
    private string RouteFilePath => Path.Combine(_root.Root, "frontend", "src", "routes", $"{NameKebab}.tsx");
    private string RootRoutePath => Path.Combine(_root.Root, "frontend", "src", "routes", "__root.tsx");
    private string StartSentinel => $"{{/* dostar:feature:{NameKebab}:start */}}";
    private string EndSentinel => $"{{/* dostar:feature:{NameKebab}:end */}}";

    private const string AnyNavSentinelMarker = "{/* dostar:feature:";

    internal Task<int> RemoveAsync()
    {
        if (!Directory.Exists(FeatureDir))
        {
            Console.Error.WriteLine($"Error: feature '{name}' does not exist at {FeatureDir}");
            return Task.FromResult(1);
        }

        PrintPlan();

        if (dryRun)
        {
            Console.WriteLine("Dry run complete. No changes were made.");
            return Task.FromResult(0);
        }

        if (!yes && !ConfirmPrompt())
        {
            Console.WriteLine("Aborted.");
            return Task.FromResult(0);
        }

        DeleteFeatureDir();
        DeleteRouteFile();
        CleanRootNavLink();
        Console.WriteLine();
        Console.WriteLine($"Feature '{name}' removed successfully.");
        return Task.FromResult(0);
    }

    private void PrintPlan()
    {
        Console.WriteLine(dryRun ? "[DRY RUN] The following changes would be made:" : "The following changes will be made:");
        Console.WriteLine();
        Console.WriteLine($"  Remove directory:  {FeatureDir}");
        if (File.Exists(RouteFilePath))
            Console.WriteLine($"  Delete route file: {Path.GetRelativePath(_root.Root, RouteFilePath)}");
        if (File.Exists(RootRoutePath) && File.ReadAllText(RootRoutePath).Contains(StartSentinel, StringComparison.Ordinal))
            Console.WriteLine($"  Update nav:        {Path.GetRelativePath(_root.Root, RootRoutePath)}");
        Console.WriteLine();
    }

    private static bool ConfirmPrompt()
    {
        Console.Write("Remove feature? [y/N] ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response is "y" or "yes";
    }

    private void DeleteFeatureDir()
    {
        Directory.Delete(FeatureDir, recursive: true);
        Console.WriteLine($"  Deleted: {FeatureDir}");
    }

    private void DeleteRouteFile()
    {
        if (!File.Exists(RouteFilePath))
            return;
        File.Delete(RouteFilePath);
        Console.WriteLine($"  Deleted route: {Path.GetRelativePath(_root.Root, RouteFilePath)}");
    }

    private void CleanRootNavLink()
    {
        if (!File.Exists(RootRoutePath))
            return;

        var content = File.ReadAllText(RootRoutePath);
        if (!content.Contains(StartSentinel, StringComparison.Ordinal))
            return;

        var lines = content.Split('\n').ToList();
        RemoveSentinelBlock(lines);

        var hasRemainingFeatures = string.Join('\n', lines).Contains(AnyNavSentinelMarker, StringComparison.Ordinal);
        if (!hasRemainingFeatures)
            RemoveLinkImport(lines);

        File.WriteAllText(RootRoutePath, string.Join('\n', lines));
        Console.WriteLine($"  Updated nav:       {Path.GetRelativePath(_root.Root, RootRoutePath)}");
    }

    private void RemoveSentinelBlock(List<string> lines)
    {
        var startIdx = lines.FindIndex(l => l.Contains(StartSentinel, StringComparison.Ordinal));
        var endIdx = lines.FindIndex(l => l.Contains(EndSentinel, StringComparison.Ordinal));
        if (startIdx >= 0 && endIdx >= startIdx)
            lines.RemoveRange(startIdx, endIdx - startIdx + 1);
    }

    private static void RemoveLinkImport(List<string> lines)
    {
        var importIdx = lines.FindIndex(l => l.Contains("from '@tanstack/react-router'", StringComparison.Ordinal));
        if (importIdx < 0) return;

        var importLine = lines[importIdx];
        if (!importLine.Contains("Link, ", StringComparison.Ordinal)) return;

        lines[importIdx] = importLine.Replace("Link, ", "", StringComparison.Ordinal);
    }
}
