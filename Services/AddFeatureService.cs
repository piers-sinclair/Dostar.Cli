namespace Dostar.Cli;

internal sealed class AddFeatureService(string name, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string NameKebab => name.ToKebabCase();
    private string NameScreaming => name.ToScreamingSnakeCase();
    private string FeaturesDir => Path.Combine(_root.Root, "frontend", "src", "features", NameKebab);
    private string IndexRoutePath => Path.Combine(_root.Root, "frontend", "src", "routes", "index.tsx");
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
        WireIndexRoute();
        return true;
    }

    private async Task GenerateFilesAsync()
    {
        var componentsDir = Path.Combine(FeaturesDir, "components");
        var hooksDir = Path.Combine(FeaturesDir, "hooks");
        var mocksDir = Path.Combine(FeaturesDir, "mocks");

        Directory.CreateDirectory(componentsDir);
        Directory.CreateDirectory(hooksDir);
        Directory.CreateDirectory(mocksDir);

        var model = new { name, name_kebab = NameKebab, name_screaming = NameScreaming };

        await TemplateRenderer.RenderAsync("handlers.ts.scriban", model, Path.Combine(mocksDir, "handlers.ts"));
        await TemplateRenderer.RenderAsync("FeatureList.tsx.scriban", model, Path.Combine(componentsDir, $"{name}List.tsx"));
        await TemplateRenderer.RenderAsync("useFeature.ts.scriban", model, Path.Combine(hooksDir, $"use{name}.ts"));

        Console.WriteLine(" Generated feature files.");
    }

    private void WireIndexRoute()
    {
        if (!File.Exists(IndexRoutePath))
        {
            Console.WriteLine($" Skipped route wiring: {Path.GetRelativePath(_root.Root, IndexRoutePath)} not found.");
            return;
        }

        var lines = File.ReadAllText(IndexRoutePath).Split('\n').ToList();

        InsertImport(lines);
        InsertSentinelBlock(lines);

        File.WriteAllText(IndexRoutePath, string.Join('\n', lines));
        Console.WriteLine($" Wired {name}List into {Path.GetRelativePath(_root.Root, IndexRoutePath)}.");
    }

    private void InsertImport(List<string> lines)
    {
        var importLine = $"import {{ {name}List }} from '@/features/{NameKebab}/components/{name}List';";
        var lastImportIdx = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].TrimStart().StartsWith("import ", StringComparison.Ordinal))
                lastImportIdx = i;
        }
        lines.Insert(lastImportIdx >= 0 ? lastImportIdx + 1 : 0, importLine);
    }

    private void InsertSentinelBlock(List<string> lines)
    {
        if (TryInsertAfterLastSentinel(lines)) return;
        if (TryReplacePlaceholderReturn(lines)) return;
        InsertBeforeClosingDiv(lines);
    }

    private bool TryInsertAfterLastSentinel(List<string> lines)
    {
        var lastEndIdx = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(":end */}", StringComparison.Ordinal))
                lastEndIdx = i;
        }
        if (lastEndIdx < 0)
            return false;
        lines.InsertRange(lastEndIdx + 1, BuildSentinelLines(LeadingWhitespace(lines[lastEndIdx])));
        return true;
    }

    private bool TryReplacePlaceholderReturn(List<string> lines)
    {
        var placeholderIdx = lines.FindIndex(l => l.Trim() == "return <></>;");
        if (placeholderIdx < 0)
            return false;
        var indent = LeadingWhitespace(lines[placeholderIdx]);
        lines.RemoveAt(placeholderIdx);
        lines.InsertRange(placeholderIdx,
        [
            $"{indent}return (",
            $"{indent}    <div className=\"mx-auto max-w-lg space-y-6\">",
            $"{indent}        {StartSentinel}",
            $"{indent}        <{name}List />",
            $"{indent}        {EndSentinel}",
            $"{indent}    </div>",
            $"{indent});",
        ]);
        return true;
    }

    private void InsertBeforeClosingDiv(List<string> lines)
    {
        var closingDivIdx = lines.FindLastIndex(l => l.Trim() == "</div>");
        if (closingDivIdx >= 0)
            lines.InsertRange(closingDivIdx, BuildSentinelLines(LeadingWhitespace(lines[closingDivIdx]) + "    "));
    }

    private string[] BuildSentinelLines(string indent) =>
    [
        $"{indent}{StartSentinel}",
        $"{indent}<{name}List />",
        $"{indent}{EndSentinel}",
    ];

    private static string LeadingWhitespace(string line) =>
        new(line.TakeWhile(char.IsWhiteSpace).ToArray());
}
