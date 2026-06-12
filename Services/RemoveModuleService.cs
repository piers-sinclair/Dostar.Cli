namespace Dostar.Cli;

internal sealed class RemoveModuleService(string name, bool dryRun, bool yes, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string Prefix => Path.GetFileNameWithoutExtension(_root.SlnxPath);
    private string ModulesDir => Path.Combine(_root.Root, "backend", "Modules", name);

    internal async Task<int> RemoveAsync()
    {
        if (!Directory.Exists(ModulesDir))
        {
            Console.Error.WriteLine($"Error: module '{name}' does not exist at {ModulesDir}");
            return 1;
        }

        WarnIfCrossReferences();
        PrintPlan();

        if (dryRun)
        {
            Console.WriteLine("Dry run complete. No changes were made.");
            return 0;
        }

        if (!yes && !ConfirmPrompt())
        {
            Console.WriteLine("Aborted.");
            return 0;
        }

        await ApplyAsync();
        Console.WriteLine();
        Console.WriteLine($"Module '{name}' removed successfully.");
        return 0;
    }

    private string[] ModuleProjectPaths() =>
    [
        Path.Combine(ModulesDir, $"{Prefix}.{name}.Contracts",         $"{Prefix}.{name}.Contracts.csproj"),
        Path.Combine(ModulesDir, $"{Prefix}.{name}.Implementation",    $"{Prefix}.{name}.Implementation.csproj"),
        Path.Combine(ModulesDir, $"{Prefix}.{name}.UnitTests",         $"{Prefix}.{name}.UnitTests.csproj"),
        Path.Combine(ModulesDir, $"{Prefix}.{name}.IntegrationTests",  $"{Prefix}.{name}.IntegrationTests.csproj"),
    ];

    private void WarnIfCrossReferences()
    {
        var modulesDir = Path.Combine(_root.Root, "backend", "Modules");
        if (!Directory.Exists(modulesDir))
            return;

        var contractsRef = $"{Prefix}.{name}.Contracts";
        var referencingProjects = Directory
            .EnumerateFiles(modulesDir, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains(Path.Combine("Modules", name) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(p => File.ReadAllText(p).Contains(contractsRef, StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .ToList();

        if (referencingProjects.Count == 0)
            return;

        Console.WriteLine($"Warning: the following projects reference {Prefix}.{name}.Contracts:");
        foreach (var proj in referencingProjects)
            Console.WriteLine($"  {proj}");
        Console.WriteLine("These references will become broken after removal.");
        Console.WriteLine();
    }

    private void PrintPlan()
    {
        var programCsPath = Path.Combine(_root.Root, "backend", $"{Prefix}.Api", "Program.cs");
        var implCsproj = $"{Prefix}.{name}.Implementation";

        Console.WriteLine(dryRun ? "[DRY RUN] The following changes would be made:" : "The following changes will be made:");
        Console.WriteLine();
        Console.WriteLine($"  Remove directory:              {ModulesDir}");
        foreach (var proj in ModuleProjectPaths())
            Console.WriteLine($"  Remove from solution:          {Path.GetRelativePath(_root.Root, proj)}");
        Console.WriteLine($"  Remove project reference:      {Prefix}.Api -> {implCsproj}");
        Console.WriteLine($"  Remove module registration:    new {name}Module() from {Path.GetRelativePath(_root.Root, programCsPath)}");
        Console.WriteLine();
    }

    private static bool ConfirmPrompt()
    {
        Console.Write("Remove module? [y/N] ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response is "y" or "yes";
    }

    private async Task ApplyAsync()
    {
        var apiCsprojPath = Path.Combine(_root.Root, "backend", $"{Prefix}.Api", $"{Prefix}.Api.csproj");
        var implCsprojPath = Path.Combine(ModulesDir, $"{Prefix}.{name}.Implementation", $"{Prefix}.{name}.Implementation.csproj");

        await SolutionCli.RemoveReferenceAsync(apiCsprojPath, implCsprojPath, _root.Root);
        await SolutionCli.RemoveProjectsAsync(_root.SlnxPath, ModuleProjectPaths(), _root.Root);
        UnregisterModuleAsync();
        Directory.Delete(ModulesDir, recursive: true);
        Console.WriteLine($"  Deleted: {ModulesDir}");
    }

    private void UnregisterModuleAsync()
    {
        var programCsPath = Path.Combine(_root.Root, "backend", $"{Prefix}.Api", "Program.cs");
        if (!File.Exists(programCsPath))
        {
            Console.Error.WriteLine($"Warning: Program.cs not found at {programCsPath}");
            return;
        }

        var content = File.ReadAllText(programCsPath).Replace("\r\n", "\n");
        var moduleRegistration = $"new {name}Module()";

        if (!content.Contains(moduleRegistration, StringComparison.Ordinal))
        {
            Console.WriteLine($"  Warning: could not find '{moduleRegistration}' in Program.cs — skipped.");
            return;
        }

        var lines = content.Split('\n').ToList();

        RemoveLine(lines, moduleRegistration);
        RemoveLine(lines, $"using {Prefix}.{name}.Implementation;");

        File.WriteAllText(programCsPath, string.Join('\n', lines));
        Console.WriteLine($"  Removed {name}Module registration from Program.cs.");
    }

    private static void RemoveLine(List<string> lines, string substring)
    {
        for (var i = lines.Count - 1; i >= 0; i--)
        {
            if (!lines[i].Contains(substring, StringComparison.Ordinal))
                continue;
            lines.RemoveAt(i);
            return;
        }
    }
}
