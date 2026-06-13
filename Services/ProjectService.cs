namespace Dostar.Cli;

internal sealed class ProjectService(string projectName, string? output, string githubOrg, string author)
{
    private const string TemplateRepoUrl = "https://github.com/piers-sinclair/Dostar.git";
    private const string GitDirectoryName = ".git";
    private const string CrossRepoDependencyMarker = "> **Cross-repo dependency:**";
    private const string TemplateMarketingMarker = "gives you a production-ready fullstack app";
    private const string GoalsSectionHeading = "## Goals";
    private const string CreateProjectStepHeading = "### 1. Create your project";
    private const string NumberedStepPrefix = "### ";

    private static readonly string[] TextExtensions =
    [
        ".cs", ".csproj", ".slnx", ".sln", ".json", ".xml", ".config", ".yaml", ".yml",
        ".md", ".txt", ".sh", ".ps1", ".ts", ".tsx", ".js", ".jsx", ".html", ".css",
        ".scss", ".env", ".gitignore", ".gitattributes", ".editorconfig", ".props",
        ".targets", ".bicep", ".bicepparam", ".http", ".razor", ".cshtml", ".toml"
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

        Console.WriteLine("Resetting changelog and version...");
        ResetVersioning(outputDir);

        Console.WriteLine("Cleaning up template-specific documentation...");
        CleanClaudeMd(outputDir);
        CleanReadmeMd(outputDir);

        Console.WriteLine("Initialising git repository...");
        await GitCli.InitAsync(outputDir, author);

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

    private string Substitute(string input, string projectNameLower) =>
        ProjectNameSubstitutor.Substitute(input, projectName, projectNameLower, githubOrg);

    private static void ResetVersioning(string rootDir)
    {
        var changelogPath = Path.Combine(rootDir, "CHANGELOG.md");
        if (File.Exists(changelogPath))
            File.WriteAllText(changelogPath, "# Changelog\n");

        var manifestPath = Path.Combine(rootDir, ".release-please-manifest.json");
        if (File.Exists(manifestPath))
            File.WriteAllText(manifestPath, "{\n  \".\": \"0.0.0\"\n}\n");
    }

    private static void CleanClaudeMd(string rootDir)
    {
        var claudeMdPath = Path.Combine(rootDir, "CLAUDE.md");
        if (!File.Exists(claudeMdPath))
            return;

        var lines = File.ReadAllLines(claudeMdPath).ToList();
        lines = RemoveCrossRepoDependencyBlock(lines);
        lines = CollapseBlankLines(lines);

        File.WriteAllLines(claudeMdPath, lines);
    }

    private static void CleanReadmeMd(string rootDir)
    {
        var readmePath = Path.Combine(rootDir, "README.md");
        if (!File.Exists(readmePath))
            return;

        var lines = File.ReadAllLines(readmePath).ToList();
        lines = RemoveLineContaining(lines, TemplateMarketingMarker);
        lines = RemoveSection(lines, GoalsSectionHeading);
        lines = RemoveSection(lines, CreateProjectStepHeading);
        lines = RenumberQuickStartSteps(lines);
        lines = CollapseBlankLines(lines);

        File.WriteAllLines(readmePath, lines);
    }

    private static List<string> RemoveCrossRepoDependencyBlock(List<string> lines)
    {
        var blockStart = lines.FindIndex(l => l.StartsWith(CrossRepoDependencyMarker, StringComparison.Ordinal));
        if (blockStart == -1)
            return lines;

        var blockEnd = blockStart + 1;
        while (blockEnd < lines.Count && lines[blockEnd].StartsWith('>'))
            blockEnd++;

        var result = new List<string>(lines);
        result.RemoveRange(blockStart, blockEnd - blockStart);
        return result;
    }

    private static List<string> RemoveLineContaining(List<string> lines, string marker) =>
        lines.Where(line => !line.Contains(marker, StringComparison.Ordinal)).ToList();

    private static List<string> RemoveSection(List<string> lines, string sectionHeading)
    {
        var spaceIndex = sectionHeading.IndexOf(' ');
        var terminatorPrefix = sectionHeading[..spaceIndex] + " ";
        var result = new List<string>(lines.Count);
        var inSection = false;
        foreach (var line in lines)
        {
            if (line == sectionHeading)
            {
                inSection = true;
                continue;
            }
            if (inSection)
            {
                if (line.StartsWith(terminatorPrefix, StringComparison.Ordinal))
                    inSection = false;
                else
                    continue;
            }
            result.Add(line);
        }
        return result;
    }

    private static List<string> RenumberQuickStartSteps(List<string> lines)
    {
        var stepNumber = 1;
        var result = new List<string>(lines.Count);
        foreach (var line in lines)
        {
            if (line.StartsWith(NumberedStepPrefix, StringComparison.Ordinal))
            {
                var afterPrefix = line[NumberedStepPrefix.Length..];
                var dotSpaceIndex = afterPrefix.IndexOf(". ", StringComparison.Ordinal);
                if (dotSpaceIndex > 0 && afterPrefix[..dotSpaceIndex].All(char.IsAsciiDigit))
                {
                    result.Add($"{NumberedStepPrefix}{stepNumber++}. {afterPrefix[(dotSpaceIndex + 2)..]}");
                    continue;
                }
            }
            result.Add(line);
        }
        return result;
    }

    private static List<string> CollapseBlankLines(List<string> lines)
    {
        var result = new List<string>(lines.Count);
        var previousLineWasBlank = false;
        foreach (var line in lines)
        {
            var isBlank = string.IsNullOrWhiteSpace(line);
            if (isBlank && previousLineWasBlank)
                continue;
            result.Add(line);
            previousLineWasBlank = isBlank;
        }
        return result;
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
