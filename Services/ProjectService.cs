namespace Dostar.Cli;

internal sealed class ProjectService(string projectName, string? output)
{
    private const string TemplateRepoUrl   = "https://github.com/piers-sinclair/Dostar.git";
    private const string GitDirectoryName  = ".git";

    private static readonly string[] TextExtensions =
    [
        ".cs", ".csproj", ".slnx", ".sln", ".json", ".xml", ".config", ".yaml", ".yml",
        ".md", ".txt", ".sh", ".ps1", ".ts", ".tsx", ".js", ".jsx", ".html", ".css",
        ".scss", ".env", ".gitignore", ".gitattributes", ".editorconfig", ".props",
        ".targets", ".bicep", ".http", ".razor", ".cshtml", ".toml"
    ];

    internal async Task CreateAsync()
    {
        var outputDir = Path.GetFullPath(output ?? Path.Combine(Directory.GetCurrentDirectory(), projectName));

        if (Directory.Exists(outputDir) && Directory.EnumerateFileSystemEntries(outputDir).Any())
            throw new InvalidOperationException($"Output directory '{outputDir}' already exists and is not empty.");

        Console.WriteLine($"Creating new project '{projectName}' in '{outputDir}'...");
        Console.WriteLine();

        await CloneTemplateAsync(outputDir);
        RemoveGitHistory(outputDir);

        Console.WriteLine("Renaming Dostar references...");
        ApplyProjectName(outputDir);
        Console.WriteLine("Renaming complete.");

        PrintSuccessMessage(outputDir);
    }

    private static async Task CloneTemplateAsync(string outputDir)
    {
        Console.WriteLine("Cloning Dostar template...");
        var exitCode = await ProcessRunner.RunAsync("git", ["clone", TemplateRepoUrl, outputDir], Directory.GetCurrentDirectory());
        if (exitCode != 0)
            throw new InvalidOperationException("git clone failed.");
    }

    private static void RemoveGitHistory(string outputDir)
    {
        Console.WriteLine("Removing template git history...");
        var gitDir = Path.Combine(outputDir, GitDirectoryName);
        if (Directory.Exists(gitDir))
            DeleteDirectoryForce(gitDir);
    }

    private void ApplyProjectName(string rootDir)
    {
        var projectNameLower = projectName.ToLowerInvariant();
        RenameEntriesBottomUp(new DirectoryInfo(rootDir), projectNameLower);
        SubstituteInFiles(rootDir, projectNameLower);
    }

    private void RenameEntriesBottomUp(DirectoryInfo dir, string projectNameLower)
    {
        foreach (var subDir in dir.GetDirectories())
        {
            if (subDir.Name == GitDirectoryName)
                continue;

            RenameEntriesBottomUp(subDir, projectNameLower);

            var newName = Substitute(subDir.Name, projectNameLower);
            if (newName != subDir.Name)
                subDir.MoveTo(Path.Combine(subDir.Parent!.FullName, newName));
        }

        foreach (var file in dir.GetFiles())
        {
            var newName = Substitute(file.Name, projectNameLower);
            if (newName != file.Name)
                file.MoveTo(Path.Combine(file.DirectoryName!, newName));
        }
    }

    private void SubstituteInFiles(string rootDir, string projectNameLower)
    {
        var files = Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories)
            .Where(f => !IsUnderGitDirectory(rootDir, f))
            .Where(IsTextFile);

        foreach (var filePath in files)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                var updated = Substitute(content, projectNameLower);
                if (updated != content)
                    File.WriteAllText(filePath, updated);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Warning: Could not process '{filePath}': {ex.Message}");
            }
        }
    }

    private string Substitute(string input, string projectNameLower) =>
        input
            .Replace("Dostar", projectName, StringComparison.Ordinal)
            .Replace("dostar", projectNameLower, StringComparison.Ordinal);

    private void PrintSuccessMessage(string outputDir)
    {
        Console.WriteLine();
        Console.WriteLine($"Project '{projectName}' created successfully at '{outputDir}'.");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine($"  cd {projectName}");
        Console.WriteLine("  git init && git add . && git commit -m \"Initial commit\"");
        Console.WriteLine("  dotnet build");
        Console.WriteLine("  cd frontend && pnpm dev");
    }

    private static bool IsUnderGitDirectory(string rootDir, string filePath) =>
        Path.GetRelativePath(rootDir, filePath)
            .StartsWith(GitDirectoryName + Path.DirectorySeparatorChar, StringComparison.Ordinal);

    private static bool IsTextFile(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (TextExtensions.Contains(ext))
            return true;

        if (string.IsNullOrEmpty(ext))
        {
            var fileName = Path.GetFileName(filePath);
            return fileName is "Dockerfile" or "Makefile" or "LICENSE" or "NOTICE" or "AUTHORS";
        }

        return false;
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
