namespace Dostar.Cli;

internal sealed class AddFeatureService(string name, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string NameKebab => name.ToKebabCase();
    private string FeaturesDir => Path.Combine(_root.Root, "frontend", "src", "features", NameKebab);
    private string RouteFilePath => Path.Combine(_root.Root, "frontend", "src", "routes", $"{NameKebab}.tsx");
    private string RootRoutePath => Path.Combine(_root.Root, "frontend", "src", "routes", "__root.tsx");
    private string StartSentinel => $"{{/* dostar:feature:{NameKebab}:start */}}";
    private string EndSentinel => $"{{/* dostar:feature:{NameKebab}:end */}}";

    internal async Task<bool> AddAsync()
    {
        if (Directory.Exists(FeaturesDir))
        {
            Console.WriteLine($"Feature '{name}' already exists at {FeaturesDir}. Nothing to do.");
            return false;
        }

        await GenerateFilesAsync();
        await CreateRouteFile();
        WireNavLink();
        return true;
    }

    private async Task GenerateFilesAsync()
    {
        Directory.CreateDirectory(Path.Combine(FeaturesDir, "components"));
        Directory.CreateDirectory(Path.Combine(FeaturesDir, "hooks"));
        var mocksDir = Path.Combine(FeaturesDir, "mocks");
        Directory.CreateDirectory(mocksDir);

        var model = new { name, name_kebab = NameKebab, name_screaming = name.ToScreamingSnakeCase() };
        await TemplateRenderer.RenderAsync("handlers.ts.scriban", model, Path.Combine(mocksDir, "handlers.ts"));

        Console.WriteLine(" Generated feature files.");
    }

    private async Task CreateRouteFile()
    {
        var model = new { name, name_kebab = NameKebab };
        await TemplateRenderer.RenderAsync("FeaturePage.tsx.scriban", model, RouteFilePath);
        Console.WriteLine($" Created route file: {Path.GetRelativePath(_root.Root, RouteFilePath)}");
    }

    private void WireNavLink()
    {
        if (!File.Exists(RootRoutePath))
        {
            Console.WriteLine($" Skipped nav link wiring: {Path.GetRelativePath(_root.Root, RootRoutePath)} not found.");
            return;
        }

        var lines = File.ReadAllText(RootRoutePath).Split('\n').ToList();
        EnsureLinkImport(lines);
        InsertNavSentinelBlock(lines);
        File.WriteAllText(RootRoutePath, string.Join('\n', lines));
        Console.WriteLine($" Wired {name} nav link into {Path.GetRelativePath(_root.Root, RootRoutePath)}.");
    }

    private static void EnsureLinkImport(List<string> lines)
    {
        var importIdx = lines.FindIndex(l => l.Contains("from '@tanstack/react-router'", StringComparison.Ordinal));
        if (importIdx < 0) return;

        var importLine = lines[importIdx];
        if (importLine.Contains(" Link", StringComparison.Ordinal)) return;

        var braceIdx = importLine.IndexOf('{');
        if (braceIdx >= 0)
            lines[importIdx] = importLine.Insert(braceIdx + 2, "Link, ");
    }

    private void InsertNavSentinelBlock(List<string> lines)
    {
        var lastEndIdx = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(":end */}", StringComparison.Ordinal))
                lastEndIdx = i;
        }

        if (lastEndIdx >= 0)
        {
            lines.InsertRange(lastEndIdx + 1, BuildNavSentinelLines(LeadingWhitespace(lines[lastEndIdx])));
            return;
        }

        var navCloseIdx = lines.FindLastIndex(l => l.Trim() == "</nav>");
        if (navCloseIdx >= 0)
            lines.InsertRange(navCloseIdx, BuildNavSentinelLines(LeadingWhitespace(lines[navCloseIdx]) + "    "));
    }

    private string[] BuildNavSentinelLines(string indent) =>
    [
        $"{indent}{StartSentinel}",
        $"{indent}<Link to=\"/{NameKebab}\" className=\"text-sm text-muted-foreground hover:text-foreground [&.active]:text-foreground [&.active]:font-medium\">{name}</Link>",
        $"{indent}{EndSentinel}",
    ];

    private static string LeadingWhitespace(string line) =>
        new(line.TakeWhile(char.IsWhiteSpace).ToArray());
}
