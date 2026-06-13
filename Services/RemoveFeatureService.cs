namespace Dostar.Cli;

internal sealed class RemoveFeatureService(string name, bool dryRun, bool yes, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string NameKebab => name.ToKebabCase();
    private string FeatureDir => Path.Combine(_root.Root, "frontend", "src", "features", NameKebab);
    private string IndexRoutePath => Path.Combine(_root.Root, "frontend", "src", "routes", "index.tsx");
    private string StartSentinel => $"{{/* dostar:feature:{NameKebab}:start */}}";
    private string EndSentinel => $"{{/* dostar:feature:{NameKebab}:end */}}";

    private const string IndexRoutePlaceholder =
        """
        import type { JSX } from 'react';
        import { createFileRoute } from '@tanstack/react-router';

        export const Route = createFileRoute('/')({
            component: IndexPage,
        });

        function IndexPage(): JSX.Element {
            return <></>;
        }
        """;

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

        DeleteFeatureAndCleanRoute();
        Console.WriteLine();
        Console.WriteLine($"Feature '{name}' removed successfully.");
        return Task.FromResult(0);
    }

    private void PrintPlan()
    {
        Console.WriteLine(dryRun ? "[DRY RUN] The following changes would be made:" : "The following changes will be made:");
        Console.WriteLine();
        Console.WriteLine($"  Remove directory:  {FeatureDir}");
        if (File.Exists(IndexRoutePath) && File.ReadAllText(IndexRoutePath).Contains(StartSentinel, StringComparison.Ordinal))
            Console.WriteLine($"  Update route:      {Path.GetRelativePath(_root.Root, IndexRoutePath)}");
        Console.WriteLine();
    }

    private static bool ConfirmPrompt()
    {
        Console.Write("Remove feature? [y/N] ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response is "y" or "yes";
    }

    private void DeleteFeatureAndCleanRoute()
    {
        Directory.Delete(FeatureDir, recursive: true);
        Console.WriteLine($"  Deleted: {FeatureDir}");
        CleanIndexRoute();
    }

    private void CleanIndexRoute()
    {
        if (!File.Exists(IndexRoutePath))
            return;

        var content = File.ReadAllText(IndexRoutePath);
        if (!content.Contains(StartSentinel, StringComparison.Ordinal))
            return;

        var lines = content.Split('\n').ToList();

        // Remove import lines from this feature
        lines.RemoveAll(l => l.Contains($"from '@/features/{NameKebab}/", StringComparison.Ordinal));

        // Remove sentinel block (start marker through end marker, inclusive)
        var startIdx = lines.FindIndex(l => l.Contains(StartSentinel, StringComparison.Ordinal));
        var endIdx = lines.FindIndex(l => l.Contains(EndSentinel, StringComparison.Ordinal));
        if (startIdx >= 0 && endIdx >= startIdx)
            lines.RemoveRange(startIdx, endIdx - startIdx + 1);

        var pruned = string.Join('\n', lines);
        var hasRemainingFeatures = pruned.Contains("{/* dostar:feature:", StringComparison.Ordinal);
        File.WriteAllText(IndexRoutePath, hasRemainingFeatures ? pruned : IndexRoutePlaceholder);
        Console.WriteLine($"  Updated route:     {Path.GetRelativePath(_root.Root, IndexRoutePath)}");
    }
}
