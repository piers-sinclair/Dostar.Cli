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
}
