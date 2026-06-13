namespace Dostar.Cli;

internal static class ReadmeCleaner
{
    private const string TemplateMarketingMarker = "gives you a production-ready fullstack app";
    private const string GoalsSectionHeading = "## Goals";
    private const string CreateProjectStepHeading = "### 1. Create your project";
    private const string NumberedStepPrefix = "### ";

    internal static void Clean(string rootDir)
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

    private static List<string> RemoveLineContaining(List<string> lines, string marker) =>
        lines.Where(line => !line.Contains(marker, StringComparison.Ordinal)).ToList();

    private static List<string> RemoveSection(List<string> lines, string sectionHeading)
    {
        var start = lines.IndexOf(sectionHeading);
        if (start == -1)
            return lines;

        var terminatorPrefix = sectionHeading[..sectionHeading.IndexOf(' ')] + " ";
        var end = lines.FindIndex(start + 1, l => l.StartsWith(terminatorPrefix, StringComparison.Ordinal));

        return end == -1
            ? [.. lines.Take(start)]
            : [.. lines.Take(start), .. lines.Skip(end)];
    }

    private static List<string> RenumberQuickStartSteps(List<string> lines)
    {
        var stepNumber = 1;
        return lines
            .Select(line => ExtractStepTitle(line) is { } title
                ? $"{NumberedStepPrefix}{stepNumber++}. {title}"
                : line)
            .ToList();
    }

    private static string? ExtractStepTitle(string line)
    {
        if (!line.StartsWith(NumberedStepPrefix, StringComparison.Ordinal))
            return null;
        var afterPrefix = line[NumberedStepPrefix.Length..];
        var dotSpaceIndex = afterPrefix.IndexOf(". ", StringComparison.Ordinal);
        return dotSpaceIndex > 0 && afterPrefix[..dotSpaceIndex].All(char.IsAsciiDigit)
            ? afterPrefix[(dotSpaceIndex + 2)..]
            : null;
    }

    private static List<string> CollapseBlankLines(List<string> lines) =>
        lines
            .Where((line, i) => i == 0 || !string.IsNullOrWhiteSpace(line) || !string.IsNullOrWhiteSpace(lines[i - 1]))
            .ToList();
}
