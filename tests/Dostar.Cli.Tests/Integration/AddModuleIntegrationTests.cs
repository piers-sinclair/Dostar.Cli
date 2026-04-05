using Dostar.Cli.Tests.Helpers;

namespace Dostar.Cli.Tests.Integration;

public class AddModuleIntegrationTests : IDisposable
{
    private readonly FakeRepo _repo = new();

    [Fact]
    public async Task AddAsync_NewModule_CreatesAllExpectedFiles()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        var modulesDir = _repo.ModulesDir("Billing");
        var p = FakeRepo.Prefix;

        Directory.Exists(modulesDir).ShouldBeTrue();

        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.Contracts",        $"{p}.Billing.Contracts.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.Implementation",   $"{p}.Billing.Implementation.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.Implementation",   "BillingModule.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.Implementation",   "GlobalUsings.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.UnitTests",        $"{p}.Billing.UnitTests.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.UnitTests",        "GlobalUsings.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.UnitTests",        "BillingModuleTests.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.IntegrationTests", $"{p}.Billing.IntegrationTests.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.IntegrationTests", "GlobalUsings.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.IntegrationTests", "BillingModuleIntegrationTests.cs")).ShouldBeTrue();
        File.Exists(Path.Combine(modulesDir, $"{p}.Billing.IntegrationTests", "ApiFactory.cs")).ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_NewModule_RegistersModuleInProgramCs()
    {
        await new AddModuleService("Billing", endpoints: true, _repo.RepoRoot).AddAsync();

        var programCs = await File.ReadAllTextAsync(_repo.ProgramCs);
        programCs.ShouldContain("using MyApp.Billing.Implementation;");
        programCs.ShouldContain("new BillingModule()");
    }

    [Fact]
    public async Task AddAsync_AlreadyExists_ReturnsFalse()
    {
        var service = new AddModuleService("Billing", endpoints: true, _repo.RepoRoot);
        await service.AddAsync();

        var result = await service.AddAsync();

        result.ShouldBeFalse();
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}
