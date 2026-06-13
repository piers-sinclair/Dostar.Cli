namespace Dostar.Cli;

internal static class ClaudeMdCleaner
{
    private const string CrossRepoDependencyMarker = "> **Cross-repo dependency:**";
    private const string TemplateFramingMarker = "is a production-ready fullstack template";

    internal static void Clean(string rootDir, string projectName)
    {
        var claudeMdPath = Path.Combine(rootDir, "CLAUDE.md");
        if (!File.Exists(claudeMdPath))
            return;

        var lines = File.ReadAllLines(claudeMdPath).ToList();
        lines = RemoveCrossRepoDependencyBlock(lines);
        lines = ReplaceTemplateIntro(lines, projectName);
        lines = CollapseBlankLines(lines);

        File.WriteAllLines(claudeMdPath, lines);
    }

    private static List<string> RemoveCrossRepoDependencyBlock(List<string> lines)
    {
        var start = lines.FindIndex(l => l.StartsWith(CrossRepoDependencyMarker, StringComparison.Ordinal));
        if (start == -1)
            return lines;

        var end = lines.FindIndex(start + 1, l => !l.StartsWith('>'));

        return end == -1
            ? [.. lines.Take(start)]
            : [.. lines.Take(start), .. lines.Skip(end)];
    }

    private static List<string> ReplaceTemplateIntro(List<string> lines, string projectName)
    {
        var index = lines.FindIndex(l => l.Contains(TemplateFramingMarker, StringComparison.Ordinal));
        if (index == -1)
            return lines;

        var result = new List<string>(lines);
        result[index] = $"{projectName} is a fullstack .NET + React application.";
        return result;
    }

    private static List<string> CollapseBlankLines(List<string> lines) =>
        lines
            .Where((line, i) => i == 0 || !string.IsNullOrWhiteSpace(line) || !string.IsNullOrWhiteSpace(lines[i - 1]))
            .ToList();
}
