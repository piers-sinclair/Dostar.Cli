namespace Dostar.Cli.Tests.Helpers;

/// <summary>
/// Creates a minimal Dostar-style repo in a temp directory for integration testing.
/// Directory structure mirrors what AddModuleService and RemoveModuleService expect:
///   {Root}/
///     {Prefix}.slnx
///     backend/
///       {Prefix}.Api/
///         {Prefix}.Api.csproj
///         Program.cs
///     frontend/
///       src/
///         features/
/// </summary>
internal sealed class FakeRepo : IDisposable
{
    internal const string Prefix = "MyApp";

    internal string Root { get; }
    internal string SlnxPath { get; }
    internal RepoRoot RepoRoot { get; }

    internal FakeRepo()
    {
        Root = Path.Combine(Path.GetTempPath(), $"dostar-test-{Guid.NewGuid():N}");
        var apiDir = Path.Combine(Root, "backend", $"{Prefix}.Api");

        Directory.CreateDirectory(apiDir);
        Directory.CreateDirectory(Path.Combine(Root, "frontend", "src", "features"));

        SlnxPath = Path.Combine(Root, $"{Prefix}.slnx");
        File.WriteAllText(SlnxPath, "<Solution>\n</Solution>\n");

        File.WriteAllText(
            Path.Combine(apiDir, $"{Prefix}.Api.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        File.WriteAllText(
            Path.Combine(apiDir, "Program.cs"),
            "IModule[] modules =\n[\n];\n",
            System.Text.Encoding.UTF8);

        RepoRoot = new RepoRoot(Root, SlnxPath);
    }

    internal string ModulesDir(string moduleName) =>
        Path.Combine(Root, "backend", "Modules", moduleName);

    internal string FeaturesDir(string featureName) =>
        Path.Combine(Root, "frontend", "src", "features", featureName.ToKebabCase());

    internal void CreateFeatureDir(string featureName) =>
        Directory.CreateDirectory(FeaturesDir(featureName));

    internal string ProgramCs =>
        Path.Combine(Root, "backend", $"{Prefix}.Api", "Program.cs");

    public void Dispose()
    {
        if (Directory.Exists(Root))
            Directory.Delete(Root, recursive: true);
    }
}
