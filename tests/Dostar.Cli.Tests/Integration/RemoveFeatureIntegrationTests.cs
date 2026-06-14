using Dostar.Cli.Tests.Helpers;

namespace Dostar.Cli.Tests.Integration;

public class RemoveFeatureIntegrationTests : IDisposable
{
    private readonly FakeRepo _repo = new();

    [Fact]
    public async Task RemoveAsync_FeatureExists_DeletesFeatureDirectory()
    {
        _repo.CreateFeatureDir("Billing");

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_FeatureWithSubdirectories_DeletesEntireTree()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_FeatureDoesNotExist_ReturnsErrorCode()
    {
        var result = await new RemoveFeatureService("NonExistent", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        result.ShouldBe(1);
    }

    [Fact]
    public async Task RemoveAsync_DryRun_LeavesFilesIntact()
    {
        _repo.CreateFeatureDir("Billing");

        await new RemoveFeatureService("Billing", dryRun: true, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Billing")).ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveAsync_FeatureExists_ReturnsSuccessCode()
    {
        _repo.CreateFeatureDir("Billing");

        var result = await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        result.ShouldBe(0);
    }

    [Fact]
    public async Task RemoveAsync_PascalCaseInput_FindsKebabCaseDirectory()
    {
        _repo.CreateFeatureDir("Billing");

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_FeatureWithRouteFile_DeletesRouteFile()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        File.Exists(_repo.RouteFilePath("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_FeatureWithoutRouteFile_DoesNotThrow()
    {
        _repo.CreateFeatureDir("Billing");

        var result = await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        result.ShouldBe(0);
    }

    [Fact]
    public async Task RemoveAsync_LastFeatureInNav_RemovesSentinelAndLinkImport()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();
        _repo.CreateFeatureDir("Todos");

        // Remove todos (pre-existing in __root.tsx) — replace root with only billing sentinel remaining
        await new RemoveFeatureService("Todos", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();
        // Now remove billing — last sentinel
        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        rootRoute.ShouldNotContain("{/* dostar:feature:");
        rootRoute.ShouldNotContain("Link,");
    }

    [Fact]
    public async Task RemoveAsync_WithOtherFeatureInNav_LeavesOtherNavLinkIntact()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();
        _repo.CreateFeatureDir("Todos");

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        rootRoute.ShouldNotContain("{/* dostar:feature:billing:");
        rootRoute.ShouldContain("{/* dostar:feature:todos:start */}");
        rootRoute.ShouldContain("{/* dostar:feature:todos:end */}");
        rootRoute.ShouldContain("Link,");
    }

    [Fact]
    public async Task RemoveAsync_DryRun_LeavesRootRouteIntact()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();
        var before = await File.ReadAllTextAsync(_repo.RootRoutePath);

        await new RemoveFeatureService("Billing", dryRun: true, yes: true, _repo.RepoRoot).RemoveAsync();

        var after = await File.ReadAllTextAsync(_repo.RootRoutePath);
        after.ShouldBe(before);
    }

    [Fact]
    public async Task RemoveAsync_FeatureNotInRootRoute_LeavesRootRouteUnchanged()
    {
        _repo.CreateFeatureDir("Billing");
        var before = await File.ReadAllTextAsync(_repo.RootRoutePath);

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var after = await File.ReadAllTextAsync(_repo.RootRoutePath);
        after.ShouldBe(before);
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}