using Dostar.Cli.Tests.Helpers;

namespace Dostar.Cli.Tests.Integration;

public class AddFeatureIntegrationTests : IDisposable
{
    private readonly FakeRepo _repo = new();

    [Fact]
    public async Task AddAsync_NewFeature_CreatesFolderStructureAndHandlers()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");

        Directory.Exists(Path.Combine(featureDir, "components")).ShouldBeTrue();
        Directory.Exists(Path.Combine(featureDir, "hooks")).ShouldBeTrue();
        Directory.Exists(Path.Combine(featureDir, "mocks")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "mocks", "handlers.ts")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeFalse();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeFalse();
    }

    [Fact]
    public async Task AddAsync_NewFeature_HandlersContainsUrlConstantsAndTypedEmptyArray()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var handlersContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "mocks", "handlers.ts"));

        handlersContent.ShouldContain("BILLING_URL");
        handlersContent.ShouldContain("BILLING_BY_ID_URL");
        handlersContent.ShouldContain("/api/v1/billing");
        handlersContent.ShouldContain("RequestHandler[]");
    }

    [Fact]
    public async Task AddAsync_MultiWordFeature_CreatesFolderWithKebabCase()
    {
        await new AddFeatureService("UserManagement", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("UserManagement");

        Directory.Exists(featureDir).ShouldBeTrue();
        featureDir.ShouldEndWith("user-management");

        var handlersContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "mocks", "handlers.ts"));

        handlersContent.ShouldContain("USER_MANAGEMENT_URL");
        handlersContent.ShouldContain("/api/v1/user-management");
        handlersContent.ShouldContain("RequestHandler[]");
    }

    [Fact]
    public async Task AddAsync_AlreadyExists_ReturnsFalse()
    {
        var service = new AddFeatureService("Billing", _repo.RepoRoot);
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
