namespace Dostar.Cli;

internal sealed class AddFeatureService(string name, RepoRoot? root = null, FeatureType type = FeatureType.List, bool yes = false)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string NameKebab => name.ToKebabCase();
    private string NameScreaming => name.ToScreamingSnakeCase();
    private string FeaturesDir => Path.Combine(_root.Root, "frontend", "src", "features", NameKebab);
    private string RouteFilePath => Path.Combine(_root.Root, "frontend", "src", "routes", $"{NameKebab}.tsx");
    private string RootRoutePath => Path.Combine(_root.Root, "frontend", "src", "routes", "__root.tsx");
    private string StartSentinel => $"{{/* dostar:feature:{NameKebab}:start */}}";
    private string EndSentinel => $"{{/* dostar:feature:{NameKebab}:end */}}";
    private string ComponentName => type == FeatureType.Form ? $"{name}Form" : $"{name}List";

    internal async Task<bool> AddAsync()
    {
        if (!Directory.Exists(FeaturesDir))
        {
            await GenerateFilesAsync();
            await CreateRouteFile();
            WireNavLink();
            return true;
        }

        if (type == FeatureType.None)
        {
            Console.WriteLine($"Feature '{name}' already exists at {FeaturesDir}. Nothing to do.");
            return false;
        }

        return await AddComponentToExistingAsync();
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

        if (type == FeatureType.List)
        {
            await TemplateRenderer.RenderAsync("FeatureList.tsx.scriban", model, Path.Combine(componentsDir, $"{name}List.tsx"));
            await TemplateRenderer.RenderAsync("useFeature.ts.scriban", model, Path.Combine(hooksDir, $"use{name}.ts"));
        }
        else if (type == FeatureType.Form)
        {
            await TemplateRenderer.RenderAsync("FeatureForm.tsx.scriban", model, Path.Combine(componentsDir, $"{name}Form.tsx"));
            await TemplateRenderer.RenderAsync("useCreateFeature.ts.scriban", model, Path.Combine(hooksDir, $"useCreate{name}.ts"));
        }

        Console.WriteLine(" Generated feature files.");
    }

    private async Task<bool> AddComponentToExistingAsync()
    {
        var componentPath = Path.Combine(FeaturesDir, "components", $"{ComponentName}.tsx");
        if (File.Exists(componentPath))
        {
            Console.WriteLine($"Component '{ComponentName}' already exists. Nothing to do.");
            return false;
        }

        Console.WriteLine($"Feature '{name}' already exists at {FeaturesDir}.");
        Console.WriteLine($"This will add {ComponentName}.tsx to the existing feature.");

        if (!yes && !ConfirmPrompt())
        {
            Console.WriteLine("Aborted.");
            return false;
        }

        var model = new { name, name_kebab = NameKebab, name_screaming = NameScreaming };
        var templateName = type == FeatureType.Form ? "FeatureForm.tsx.scriban" : "FeatureList.tsx.scriban";
        await TemplateRenderer.RenderAsync(templateName, model, componentPath);

        if (type == FeatureType.List)
        {
            var hookPath = Path.Combine(FeaturesDir, "hooks", $"use{name}.ts");
            if (!File.Exists(hookPath))
                await TemplateRenderer.RenderAsync("useFeature.ts.scriban", model, hookPath);
        }
        else if (type == FeatureType.Form)
        {
            var hookPath = Path.Combine(FeaturesDir, "hooks", $"useCreate{name}.ts");
            if (!File.Exists(hookPath))
                await TemplateRenderer.RenderAsync("useCreateFeature.ts.scriban", model, hookPath);
        }

        Console.WriteLine($" Generated {ComponentName}.tsx.");
        return true;
    }

    private static bool ConfirmPrompt()
    {
        Console.Write("Add to existing feature? [y/N] ");
        var response = Console.ReadLine()?.Trim().ToLowerInvariant();
        return response is "y" or "yes";
    }

    private async Task CreateRouteFile()
    {
        var model = new { name, name_kebab = NameKebab, name_screaming = NameScreaming, type = type.ToString().ToLowerInvariant() };
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
