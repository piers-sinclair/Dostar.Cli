using Dostar.Cli.Tests.Helpers;

namespace Dostar.Cli.Tests.Integration;

public class AddFeatureIntegrationTests : IDisposable
{
    private readonly FakeRepo _repo = new();

    [Fact]
    public async Task AddAsync_NewFeature_CreatesFolderStructureAndFiles()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");

        Directory.Exists(Path.Combine(featureDir, "components")).ShouldBeTrue();
        Directory.Exists(Path.Combine(featureDir, "hooks")).ShouldBeTrue();
        Directory.Exists(Path.Combine(featureDir, "mocks")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "mocks", "handlers.ts")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeTrue();
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
    public async Task AddAsync_NewFeature_ListComponentContainsHookAndCard()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var componentContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "components", "BillingList.tsx"));

        componentContent.ShouldContain("useBilling");
        componentContent.ShouldContain("BillingList");
        componentContent.ShouldContain("CardTitle");
    }

    [Fact]
    public async Task AddAsync_NewFeature_HookContainsQueryAndApiPath()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var hookContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "hooks", "useBilling.ts"));

        hookContent.ShouldContain("useBilling");
        hookContent.ShouldContain("/api/v1/billing");
        hookContent.ShouldContain("useQuery");
    }

    [Fact]
    public async Task AddAsync_NewFeature_WiresIntoIndexRouteWithSentinels()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldContain("import { BillingList }");
        indexRoute.ShouldContain("{/* dostar:feature:billing:start */}");
        indexRoute.ShouldContain("<BillingList />");
        indexRoute.ShouldContain("{/* dostar:feature:billing:end */}");
    }

    [Fact]
    public async Task AddAsync_NewFeature_SentinelAppearsAfterExistingFeature()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        var todosEndIdx = indexRoute.IndexOf("{/* dostar:feature:todos:end */}", StringComparison.Ordinal);
        var billingStartIdx = indexRoute.IndexOf("{/* dostar:feature:billing:start */}", StringComparison.Ordinal);

        todosEndIdx.ShouldBeLessThan(billingStartIdx);
    }

    [Fact]
    public async Task AddAsync_NoIndexRoute_SkipsRouteWiringAndReturnsTrue()
    {
        File.Delete(_repo.IndexRoutePath);

        var result = await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        result.ShouldBeTrue();
        File.Exists(Path.Combine(_repo.FeaturesDir("Billing"), "components", "BillingList.tsx")).ShouldBeTrue();
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

    // --type none

    [Fact]
    public async Task AddAsync_NoneType_CreatesFolderStructureAndHandlersOnly()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.None).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");

        Directory.Exists(Path.Combine(featureDir, "components")).ShouldBeTrue();
        Directory.Exists(Path.Combine(featureDir, "hooks")).ShouldBeTrue();
        Directory.Exists(Path.Combine(featureDir, "mocks")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "mocks", "handlers.ts")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeFalse();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeFalse();
    }

    [Fact]
    public async Task AddAsync_NoneType_DoesNotWireIndexRoute()
    {
        var before = await File.ReadAllTextAsync(_repo.IndexRoutePath);

        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.None).AddAsync();

        var after = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        after.ShouldBe(before);
    }

    [Fact]
    public async Task AddAsync_NoneTypeAlreadyExists_ReturnsFalse()
    {
        var service = new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.None);
        await service.AddAsync();
        var result = await service.AddAsync();
        result.ShouldBeFalse();
    }

    // --type form

    [Fact]
    public async Task AddAsync_FormType_CreatesFormComponentWithoutHook()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");

        File.Exists(Path.Combine(featureDir, "components", "BillingForm.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeFalse();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeFalse();
    }

    [Fact]
    public async Task AddAsync_FormType_FormComponentContainsFormElements()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var content = await File.ReadAllTextAsync(Path.Combine(featureDir, "components", "BillingForm.tsx"));

        content.ShouldContain("BillingForm");
        content.ShouldContain("useForm");
        content.ShouldContain("handleSubmit");
        content.ShouldContain("zodResolver");
    }

    [Fact]
    public async Task AddAsync_FormType_WiresFormIntoIndexRoute()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldContain("import { BillingForm }");
        indexRoute.ShouldContain("{/* dostar:feature:billing:start */}");
        indexRoute.ShouldContain("<BillingForm />");
        indexRoute.ShouldContain("{/* dostar:feature:billing:end */}");
    }

    [Fact]
    public async Task AddAsync_FormTypeAlreadyExists_ReturnsFalse()
    {
        var service = new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form);
        await service.AddAsync();
        var result = await service.AddAsync();
        result.ShouldBeFalse();
    }

    // Adding a component type to an existing feature

    [Fact]
    public async Task AddAsync_AddFormToExistingListFeature_CreatesFormComponentAndReturnsTrue()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List).AddAsync();

        var result = await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        result.ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingForm.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_AddFormToExistingListFeature_WiresFormInsideExistingSentinel()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List).AddAsync();
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        var startIdx = indexRoute.IndexOf("{/* dostar:feature:billing:start */}", StringComparison.Ordinal);
        var endIdx = indexRoute.IndexOf("{/* dostar:feature:billing:end */}", StringComparison.Ordinal);
        var listIdx = indexRoute.IndexOf("<BillingList />", StringComparison.Ordinal);
        var formIdx = indexRoute.IndexOf("<BillingForm />", StringComparison.Ordinal);

        indexRoute.ShouldContain("import { BillingForm }");
        startIdx.ShouldBeLessThan(listIdx);
        listIdx.ShouldBeLessThan(formIdx);
        formIdx.ShouldBeLessThan(endIdx);
        indexRoute.ShouldNotContain("{/* dostar:feature:billing:start */}{/* dostar:feature:billing:start */}");
    }

    [Fact]
    public async Task AddAsync_AddListToExistingFormFeature_CreatesListComponentAndHook()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var result = await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        result.ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeTrue();
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}
