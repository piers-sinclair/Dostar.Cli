namespace Dostar.Cli.Tests.Unit;

public class TemplateRendererTests : IDisposable
{
    private readonly string _tempFile = Path.GetTempFileName();

    [Fact]
    public async Task RenderAsync_ModuleTemplateWithEndpoints_ContainsIEndpointModule()
    {
        var model = new { name = "Billing", prefix = "MyApp", endpoints = true, targetFramework = "net10.0" };

        await TemplateRenderer.RenderAsync("Module.cs.scriban", model, _tempFile);

        var content = await File.ReadAllTextAsync(_tempFile);
        content.ShouldContain("IEndpointModule");
        content.ShouldContain("MapEndpoints");
        content.ShouldNotContain("IModule\n");
    }

    [Fact]
    public async Task RenderAsync_ModuleTemplateWithoutEndpoints_ContainsIModule()
    {
        var model = new { name = "Billing", prefix = "MyApp", endpoints = false, targetFramework = "net10.0" };

        await TemplateRenderer.RenderAsync("Module.cs.scriban", model, _tempFile);

        var content = await File.ReadAllTextAsync(_tempFile);
        content.ShouldContain(": IModule");
        content.ShouldNotContain("IEndpointModule");
        content.ShouldNotContain("MapEndpoints");
    }

    [Fact]
    public async Task RenderAsync_ModuleTemplate_SubstitutesNameAndPrefix()
    {
        var model = new { name = "Billing", prefix = "MyApp", endpoints = true, targetFramework = "net10.0" };

        await TemplateRenderer.RenderAsync("Module.cs.scriban", model, _tempFile);

        var content = await File.ReadAllTextAsync(_tempFile);
        content.ShouldContain("namespace MyApp.Billing.Implementation");
        content.ShouldContain("class BillingModule");
    }

    [Fact]
    public async Task RenderAsync_ContractsTemplate_SubstitutesTargetFramework()
    {
        var model = new { name = "Billing", prefix = "MyApp", endpoints = true, targetFramework = "net10.0" };

        await TemplateRenderer.RenderAsync("Contracts.csproj.scriban", model, _tempFile);

        var content = await File.ReadAllTextAsync(_tempFile);
        content.ShouldContain("<TargetFramework>net10.0</TargetFramework>");
    }

    [Fact]
    public async Task RenderAsync_UnitTestsTemplate_PinsXunitRunnerVersion()
    {
        var model = new { name = "Billing", prefix = "MyApp", targetFramework = "net10.0" };

        await TemplateRenderer.RenderAsync("UnitTests.csproj.scriban", model, _tempFile);

        var content = await File.ReadAllTextAsync(_tempFile);
        content.ShouldContain("xunit.runner.visualstudio\" Version=\"3.1.5\"");
    }

    [Fact]
    public async Task RenderAsync_IntegrationTestsTemplate_PinsXunitRunnerVersion()
    {
        var model = new { name = "Billing", prefix = "MyApp", targetFramework = "net10.0" };

        await TemplateRenderer.RenderAsync("IntegrationTests.csproj.scriban", model, _tempFile);

        var content = await File.ReadAllTextAsync(_tempFile);
        content.ShouldContain("xunit.runner.visualstudio\" Version=\"3.1.5\"");
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
        GC.SuppressFinalize(this);
    }
}
