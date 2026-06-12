namespace Dostar.Cli;

internal sealed class AddFeatureService(string name, RepoRoot? root = null)
{
    private readonly RepoRoot _root = root ?? RepoRoot.Find();
    private string NameKebab => name.ToKebabCase();
    private string NameScreaming => name.ToScreamingSnakeCase();
    private string FeaturesDir => Path.Combine(_root.Root, "frontend", "src", "features", NameKebab);

    internal async Task<bool> AddAsync()
    {
        if (Directory.Exists(FeaturesDir))
        {
            Console.WriteLine($"Feature '{name}' already exists at {FeaturesDir}. Nothing to do.");
            return false;
        }

        await GenerateFilesAsync();
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

        await Task.WhenAll(
            TemplateRenderer.RenderAsync("FeatureList.tsx.scriban", model, Path.Combine(componentsDir, $"{name}List.tsx")),
            TemplateRenderer.RenderAsync("FeatureList.test.tsx.scriban", model, Path.Combine(componentsDir, $"{name}List.test.tsx")),
            TemplateRenderer.RenderAsync("useFeature.ts.scriban", model, Path.Combine(hooksDir, $"use{name}.ts")),
            TemplateRenderer.RenderAsync("handlers.ts.scriban", model, Path.Combine(mocksDir, "handlers.ts")));

        Console.WriteLine(" Generated feature files.");
    }
}
