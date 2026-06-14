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
///         routes/
///           index.tsx     ← clean placeholder (no feature wiring)
///           __root.tsx    ← pre-populated with Link import and todos nav sentinel
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
        var routesDir = Path.Combine(Root, "frontend", "src", "routes");

        Directory.CreateDirectory(apiDir);
        Directory.CreateDirectory(Path.Combine(Root, "frontend", "src", "features"));
        Directory.CreateDirectory(routesDir);

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

        File.WriteAllText(
            Path.Combine(routesDir, "index.tsx"),
            """
            import type { JSX } from 'react';
            import { createFileRoute } from '@tanstack/react-router';

            export const Route = createFileRoute('/')({
                component: IndexPage,
            });

            function IndexPage(): JSX.Element {
                return <></>;
            }
            """);

        File.WriteAllText(
            Path.Combine(routesDir, "__root.tsx"),
            """
            import type { JSX } from 'react';
            import { Link, createRootRoute, Outlet } from '@tanstack/react-router';

            export const Route = createRootRoute({
                component: RootLayout,
            });

            function RootLayout(): JSX.Element {
                return (
                    <>
                        <nav className="flex items-center gap-6 border-b bg-background px-8 py-4">
                            <h1 className="text-lg font-semibold text-foreground">MyApp</h1>
                            {/* dostar:feature:todos:start */}
                            <Link to="/todos" className="text-sm text-muted-foreground hover:text-foreground [&.active]:text-foreground [&.active]:font-medium">Todos</Link>
                            {/* dostar:feature:todos:end */}
                        </nav>
                        <main className="min-h-screen bg-background p-8">
                            <Outlet />
                        </main>
                    </>
                );
            }
            """);

        RepoRoot = new RepoRoot(Root, SlnxPath);
    }

    internal string ModulesDir(string moduleName) =>
        Path.Combine(Root, "backend", "Modules", moduleName);

    internal string FeaturesDir(string featureName) =>
        Path.Combine(Root, "frontend", "src", "features", featureName.ToKebabCase());

    internal string IndexRoutePath =>
        Path.Combine(Root, "frontend", "src", "routes", "index.tsx");

    internal string RootRoutePath =>
        Path.Combine(Root, "frontend", "src", "routes", "__root.tsx");

    internal string RouteFilePath(string featureName) =>
        Path.Combine(Root, "frontend", "src", "routes", $"{featureName.ToKebabCase()}.tsx");

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
