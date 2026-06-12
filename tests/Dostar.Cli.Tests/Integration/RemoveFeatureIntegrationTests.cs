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
    public async Task RemoveAsync_MultiWordFeature_DeletesKebabCaseDirectory()
    {
        _repo.CreateFeatureDir("UserManagement");

        await new RemoveFeatureService("UserManagement", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("UserManagement")).ShouldBeFalse();
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}
