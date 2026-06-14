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
    public async Task AddAsync_NewFeature_HookContainsQueryAndApiPathAndTodoComment()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var hookContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "hooks", "useBilling.ts"));

        hookContent.ShouldContain("useBilling");
        hookContent.ShouldContain("/api/v1/billing");
        hookContent.ShouldContain("useQuery");
        hookContent.ShouldContain("TODO: Update this path");
    }

    [Fact]
    public async Task AddAsync_NewFeature_CreatesRouteFile()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot).AddAsync();

        var routeFile = await File.ReadAllTextAsync(_repo.RouteFilePath("Billing"));
        routeFile.ShouldContain("createFileRoute('/billing')");
        routeFile.ShouldContain("BillingPage");
        routeFile.ShouldContain("BillingList");
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
        var linkImportCount = rootRoute.Split("import { Link").Length - 1;
        linkImportCount.ShouldBe(1);
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
        File.Exists(Path.Combine(_repo.FeaturesDir("Billing"), "components", "BillingList.tsx")).ShouldBeTrue();
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
        handlersContent.ShouldContain("RequestHandler[]");
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
    public async Task AddAsync_NoneType_CreatesRouteFileWithHeadingAndWiresNavLink()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.None).AddAsync();

        var routeFile = await File.ReadAllTextAsync(_repo.RouteFilePath("Billing"));
        routeFile.ShouldContain("createFileRoute('/billing')");
        routeFile.ShouldContain("BillingPage");
        routeFile.ShouldContain("Billing</h1>");

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        rootRoute.ShouldContain("{/* dostar:feature:billing:start */}");
        rootRoute.ShouldContain("<Link to=\"/billing\"");
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
    public async Task AddAsync_FormType_CreatesFormComponentAndMutationHook()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");

        File.Exists(Path.Combine(featureDir, "components", "BillingForm.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeFalse();
        File.Exists(Path.Combine(featureDir, "hooks", "useCreateBilling.ts")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeFalse();
    }

    [Fact]
    public async Task AddAsync_FormType_MutationHookContainsMutationAndApiPath()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var hookContent = await File.ReadAllTextAsync(Path.Combine(featureDir, "hooks", "useCreateBilling.ts"));

        hookContent.ShouldContain("useCreateBilling");
        hookContent.ShouldContain("/api/v1/billing");
        hookContent.ShouldContain("useMutation");
        hookContent.ShouldContain("invalidateQueries");
        hookContent.ShouldContain("TODO: Update this path");
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
    public async Task AddAsync_FormType_FormComponentDoesNotContainUnusedValuesParameter()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        var content = await File.ReadAllTextAsync(Path.Combine(featureDir, "components", "BillingForm.tsx"));

        content.ShouldNotContain("_values");
        content.ShouldContain("reset");
        content.ShouldContain("mapProblemDetailsErrors");
        content.ShouldContain("errors.root");
    }

    [Fact]
    public async Task AddAsync_FormType_CreatesRouteFileWithFormComponent()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var routeFile = await File.ReadAllTextAsync(_repo.RouteFilePath("Billing"));
        routeFile.ShouldContain("createFileRoute('/billing')");
        routeFile.ShouldContain("BillingPage");
        routeFile.ShouldContain("BillingForm");
        routeFile.ShouldNotContain("BillingList");
    }

    [Fact]
    public async Task AddAsync_FormTypeAlreadyExists_ReturnsFalse()
    {
        var service = new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form, yes: true);
        await service.AddAsync();
        var result = await service.AddAsync();
        result.ShouldBeFalse();
    }

    // Adding a component type to an existing feature (--yes bypasses prompt)

    [Fact]
    public async Task AddAsync_AddFormToExistingListFeature_CreatesFormComponentAndMutationHook()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List).AddAsync();

        var result = await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form, yes: true).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        result.ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingForm.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "hooks", "useCreateBilling.ts")).ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_AddFormToExistingListFeature_DoesNotAddSecondRouteOrNavLink()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List).AddAsync();
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form, yes: true).AddAsync();

        var rootRoute = await File.ReadAllTextAsync(_repo.RootRoutePath);
        var billingStartCount = rootRoute.Split(["{/* dostar:feature:billing:start */}"], StringSplitOptions.None).Length - 1;
        billingStartCount.ShouldBe(1);
    }

    [Fact]
    public async Task AddAsync_AddListToExistingFormFeature_CreatesListComponentAndHook()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form).AddAsync();

        var result = await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List, yes: true).AddAsync();

        var featureDir = _repo.FeaturesDir("Billing");
        result.ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "components", "BillingList.tsx")).ShouldBeTrue();
        File.Exists(Path.Combine(featureDir, "hooks", "useBilling.ts")).ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsync_ExistingFeatureWithoutYes_ReturnsFalse()
    {
        await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.List).AddAsync();

        var result = await new AddFeatureService("Billing", _repo.RepoRoot, FeatureType.Form, yes: false).AddAsync();

        result.ShouldBeFalse();
        File.Exists(Path.Combine(_repo.FeaturesDir("Billing"), "components", "BillingForm.tsx")).ShouldBeFalse();
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}