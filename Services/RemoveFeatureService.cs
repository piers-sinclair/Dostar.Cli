namespace Dostar.Cli;

internal sealed class RemoveFeatureService(string name, bool dryRun, bool yes, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string FeatureDir => Path.Combine(_root.Root, "frontend", "src", "features", name.ToLowerInvariant());

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

        Apply();
        Console.WriteLine();
        Console.WriteLine($"Feature '{name}' removed successfully.");
        return Task.FromResult(0);
    }

    private void PrintPlan()
    {
        Console.WriteLine(dryRun ? "[DRY RUN] The following changes would be made:" : "The following changes will be made:");
        Console.WriteLine();
        Console.WriteLine($"  Remove directory:  {FeatureDir}");
        Console.WriteLine();
    }

    private static bool ConfirmPrompt()
    {
        Console.Write("Remove feature? [y/N] ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response is "y" or "yes";
    }

    private void Apply()
    {
        Directory.Delete(FeatureDir, recursive: true);
        Console.WriteLine($"  Deleted: {FeatureDir}");
    }
}
