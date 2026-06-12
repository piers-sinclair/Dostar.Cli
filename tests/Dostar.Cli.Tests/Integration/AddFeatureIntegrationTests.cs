using Dostar.Cli.Tests.Helpers;

namespace Dostar.Cli.Tests.Integration;

public class AddFeatureIntegrationTests : IDisposable
{
    private readonly FakeRepo _repo = new();

    [Fact]
    public async Task AddAsync_NewFeature_CreatesAllExpectedFiles()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");

        Directory.Exists(featureDir).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.test.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "mocks", "handlers.ts")).ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_NewFeature_GeneratesCorrectImportPaths()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var componentContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "components", "BillingList.tsx"));
        var testContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "components", "BillingList.test.tsx"));
        var hookContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "hooks", "useBilling.ts"));
        var handlersContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "mocks", "handlers.ts"));

        componentContent.ShouldContain("@/features/billing/hooks/useBilling");
        componentContent.ShouldContain("@/shared/components/ui/card");
        testContent.ShouldContain("defaultBillingItems");
        testContent.ShouldContain("@/features/billing/mocks/handlers");
        hookContent.ShouldContain("BILLING_API_PATH");
        hookContent.ShouldContain("BILLING_QUERY_KEY");
        hookContent.ShouldContain("/api/v1/billing");
        handlersContent.ShouldContain("BILLING_URL");
        handlersContent.ShouldContain("/api/v1/billing");
        handlersContent.ShouldContain("defaultBillingItems");
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
