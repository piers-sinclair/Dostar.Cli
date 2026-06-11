namespace Dostar.Cli;

internal sealed class ProjectService(string projectName, string? output, string githubOrg, string author)
{
    private const string TemplateRepoUrl    = "https://github.com/piers-sinclair/Dostar.git";
    private const string GitDirectoryName   = ".git";

    private static readonly string[] TextExtensions =
    [
        ".cs", ".csproj", ".slnx", ".sln", ".json", ".xml", ".config", ".yaml", ".yml",
        ".md", ".txt", ".sh", ".ps1", ".ts", ".tsx", ".js", ".jsx", ".html", ".css",
        ".scss", ".env", ".gitignore", ".gitattributes", ".editorconfig", ".props",
        ".targets", ".bicep", ".http", ".razor", ".cshtml", ".toml"
    ];

    internal async Task<string> CreateAsync()
    {
        var outputDir = Path.GetFullPath(output ?? Path.Combine(Directory.GetCurrentDirectory(), projectName));

        if (Directory.Exists(outputDir) && Directory.EnumerateFileSystemEntries(outputDir).Any())
            throw new InvalidOperationException($"Output directory '{outputDir}' already exists and is not empty.");

        Console.WriteLine($"Creating new project '{projectName}' in '{outputDir}'...");
        Console.WriteLine();

        Console.WriteLine("Cloning Dostar template...");
        await GitCli.CloneAsync(TemplateRepoUrl, outputDir);

        Console.WriteLine("Removing template git history...");
        GitCli.RemoveHistory(outputDir);

        Console.WriteLine("Renaming Dostar references...");
        ApplyProjectName(outputDir);
        Console.WriteLine("Renaming complete.");

        return outputDir;
    }

    private void ApplyProjectName(string rootDir)
    {
        var projectNameLower = projectName.ToLowerInvariant();
        RenameEntriesBottomUp(new DirectoryInfo(rootDir), projectNameLower);
        SubstituteInFiles(rootDir, projectNameLower);
        ApplyLicenseMetadata(rootDir);
    }

    private void ApplyLicenseMetadata(string rootDir)
    {
        var licensePath = Path.Combine(rootDir, "LICENSE");
        if (!File.Exists(licensePath))
            return;

        var content = File.ReadAllText(licensePath);
        var updated = content
            .Replace("Piers Sinclair", author)
            .Replace("2025", DateTime.UtcNow.Year.ToString(System.Globalization.CultureInfo.InvariantCulture));

        if (updated != content)
            File.WriteAllText(licensePath, updated);
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

    private string Substitute(string input, string projectNameLower)
    {
        const string cliProtect = "\x01DOSTAR_CLI_REF\x01";
        return input
            .Replace("piers-sinclair/Dostar.Cli", cliProtect)
            .Replace("piers-sinclair/Dostar", $"{githubOrg}/Dostar")
            .Replace("\"piers-sinclair\"", $"\"{githubOrg}\"")
            .Replace("Dostar", projectName, StringComparison.Ordinal)
            .Replace("dostar", projectNameLower, StringComparison.Ordinal)
            .Replace(cliProtect, "piers-sinclair/Dostar.Cli");
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
}
