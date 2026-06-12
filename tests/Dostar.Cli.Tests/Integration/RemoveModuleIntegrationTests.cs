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
    public async Task RemoveAsync_AfterAdd_WhenFrontendFolderExists_DeletesFrontendFeatureFolder()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();
        Directory.CreateDirectory(_repo.FrontendFeaturesDir("Billing"));

        await new RemoveModuleService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FrontendFeaturesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_AfterAdd_WhenFrontendFolderAbsent_Succeeds()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        var result = await new RemoveModuleService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        result.ShouldBe(0);
        Directory.Exists(_repo.ModulesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_DryRun_LeavesFilesIntact()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        await new RemoveModuleService("Billing", dryRun: true, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.ModulesDir("Billing")).ShouldBeTrue();
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}
