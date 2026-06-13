using Dostar.Cli.Tests.Helpers;

namespace Dostar.Cli.Tests.Integration;

public class RemoveModuleIntegrationTests : IDisposable
{
    private readonly FakeRepo _repo = new();

    [Fact]
    public async Task RemoveAsync_AfterAdd_DeletesModuleDirectory()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        await new RemoveModuleService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.ModulesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_AfterAdd_UnregistersModuleFromProgramCs()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        await new RemoveModuleService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var programCs = await File.ReadAllTextAsync(_repo.ProgramCs);
        programCs.ShouldNotContain("using MyApp.Billing.Implementation;");
        programCs.ShouldNotContain("new BillingModule()");
    }

    [Fact]
    public async Task RemoveAsync_ModuleDoesNotExist_ReturnsErrorCode()
    {
        var result = await new RemoveModuleService("NonExistent", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        result.ShouldBe(1);
    }

    [Fact]
    public async Task RemoveAsync_DryRun_LeavesFilesIntact()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        await new RemoveModuleService("Billing", dryRun: true, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.ModulesDir("Billing")).ShouldBeTrue();
    }

    [Fact]
    public async Task RemoveAsync_WithFrontendFeature_DeletesFeatureDir()
    {
        await new AddModuleService("Todos", endpoints: true, _repo.RepoRoot).AddAsync();
        _repo.CreateFeatureDir("Todos");

        await new RemoveModuleService("Todos", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Todos")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_WithFrontendFeature_ResetsIndexRoute()
    {
        await new AddModuleService("Todos", endpoints: true, _repo.RepoRoot).AddAsync();
        _repo.CreateFeatureDir("Todos");

        await new RemoveModuleService("Todos", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldNotContain("@/features/todos/");
        indexRoute.ShouldNotContain("TodoList");
    }

    [Fact]
    public async Task RemoveAsync_DryRun_LeavesFrontendIntact()
    {
        await new AddModuleService("Todos", endpoints: true, _repo.RepoRoot).AddAsync();
        _repo.CreateFeatureDir("Todos");

        await new RemoveModuleService("Todos", dryRun: true, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Todos")).ShouldBeTrue();
        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldContain("@/features/todos/");
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}
