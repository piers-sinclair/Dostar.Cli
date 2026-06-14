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
    }

    [Fact]
    public async Task AddAsync_NewFeature_HandlersContainsUrlConstantsAndTypedEmptyArray()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var handlersContent = await File.ReadAllTextAsync(
            Path.Combine(_repo.FeaturesDir("Billing"), "mocks", "handlers.ts"));

        handlersContent.ShouldContain("BILLING_URL");
        handlersContent.ShouldContain("BILLING_BY_ID_URL");
        handlersContent.ShouldContain("/api/v1/billing");
        handlersContent.ShouldContain("RequestHandler[]");
    }

    [Fact]
    public async Task AddAsync_NewFeature_CreatesRouteFileImportingComponent()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var routeFile = await File.ReadAllTextAsync(_repo.RouteFilePath("Billing"));
        routeFile.ShouldContain("createFileRoute('/billing')");
        routeFile.ShouldContain("BillingPage");
        routeFile.ShouldContain("@/features/billing/components/BillingPage");
    }

    [Fact]
    public async Task AddAsync_NewFeature_CreatesComponentFileWithHeading()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var componentFile = await File.ReadAllTextAsync(
            Path.Combine(_repo.FeaturesDir("Billing"), "components", "BillingPage.tsx"));
        componentFile.ShouldContain("export function BillingPage");
        componentFile.ShouldContain("Billing</h1>");
    }

    [Fact]
    public async Task AddAsync_NewFeature_WiresNavLinkIntoRootRoute()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        rootRoute.ShouldContain("{/* dostar:feature:billing:start */}");
        rootRoute.ShouldContain("<Link to=\"/billing\"");
        rootRoute.ShouldContain("Billing");
        rootRoute.ShouldContain("{/* dostar:feature:billing:end */}");
    }

    [Fact]
    public async Task AddAsync_NewFeature_AddsLinkImportToRootRouteWhenMissing()
    {
        await File.WriteAllTextAsync(_repo.RootRoutePath,
            (await File.ReadAllTextAsync(_repo.RootRoutePath))
                .Replace("Link, ", "", StringComparison.Ordinal));

        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        rootRoute.ShouldContain("import { Link,");
    }

    [Fact]
    public async Task AddAsync_NewFeature_DoesNotDuplicateLinkImportWhenAlreadyPresent()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        rootRoute.Split("import { Link").Length.ShouldBe(2);
    }

    [Fact]
    public async Task AddAsync_SecondFeature_NavLinkAppearsAfterExistingNavLink()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        var todosEndIdx = rootRoute.IndexOf("{/* dostar:feature:todos:end */}", StringComparison.Ordinal);
        var billingStartIdx = rootRoute.IndexOf("{/* dostar:feature:billing:start */}", StringComparison.Ordinal);

        todosEndIdx.ShouldBeLessThan(billingStartIdx);
    }

    [Fact]
    public async Task AddAsync_NoRootRoute_SkipsNavWiringAndReturnsTrue()
    {
        File.Delete(_repo.RootRoutePath);

        var result = await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        result.ShouldBeTrue();
        File.Exists(_repo.RouteFilePath("Billing")).ShouldBeTrue();
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
    }

    [Fact]
    public async Task AddAsync_MultiWordFeature_CreatesRouteFileWithKebabCasePath()
    {
        await new AddFeatureService("UserManagement", _repo.RepoRoot).AddAsync();

        var routeFile = await File.ReadAllTextAsync(_repo.RouteFilePath("UserManagement"));
        routeFile.ShouldContain("createFileRoute('/user-management')");
        routeFile.ShouldContain("UserManagementPage");
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
