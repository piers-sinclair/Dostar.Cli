namespace Dostar.Cli;

internal static class SolutionCli
{
    internal static async Task AddProjectsAsync(string slnxPath, string solutionFolder, string[] projectPaths, string repoRoot)
    {
        foreach (var project in projectPaths)
        {
            var result = await ProcessRunner.RunAsync(
                "dotnet",
                ["sln", slnxPath, "add", "--solution-folder", solutionFolder, project],
                repoRoot);

            if (result != 0)
                Console.Error.WriteLine($"Warning: 'dotnet sln add' exited with code {result} for {project}");
        }

        Console.WriteLine($"  Added {projectPaths.Length} projects to {Path.GetFileName(slnxPath)}.");
    }

    internal static async Task RemoveProjectsAsync(string slnxPath, string[] projectPaths, string repoRoot)
    {
        foreach (var project in projectPaths)
        {
            if (!File.Exists(project))
            {
                Console.WriteLine($"  Skipping (not found): {Path.GetFileName(project)}");
                continue;
            }

            var result = await ProcessRunner.RunAsync("dotnet", ["sln", slnxPath, "remove", project], repoRoot);
            if (result != 0)
                Console.Error.WriteLine($"Warning: 'dotnet sln remove' exited with code {result} for {project}");
        }

        Console.WriteLine($"  Removed projects from {Path.GetFileName(slnxPath)}.");
    }

    internal static async Task AddReferenceAsync(string csprojPath, string referencePath, string repoRoot)
    {
        var result = await ProcessRunner.RunAsync("dotnet", ["add", csprojPath, "reference", referencePath], repoRoot);
        if (result != 0)
            Console.Error.WriteLine($"Warning: 'dotnet add reference' exited with code {result}.");
        else
            Console.WriteLine($"  Added reference to {Path.GetFileName(referencePath)}.");
    }

    internal static async Task RemoveReferenceAsync(string csprojPath, string referencePath, string repoRoot)
    {
        var result = await ProcessRunner.RunAsync("dotnet", ["remove", csprojPath, "reference", referencePath], repoRoot);
        if (result != 0)
            Console.Error.WriteLine($"Warning: 'dotnet remove reference' exited with code {result}.");
        else
            Console.WriteLine($"  Removed reference to {Path.GetFileName(referencePath)}.");
    }
}
