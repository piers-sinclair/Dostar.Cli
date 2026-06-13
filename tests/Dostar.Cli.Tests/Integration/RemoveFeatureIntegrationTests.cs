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
    public async Task RemoveAsync_PascalCaseInput_FindsKebabCaseDirectory()
    {
        _repo.CreateFeatureDir("Billing");

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        Directory.Exists(_repo.FeaturesDir("Billing")).ShouldBeFalse();
    }

    [Fact]
    public async Task RemoveAsync_LastFeatureInRoute_ResetsIndexRouteToPlaceholder()
    {
        _repo.CreateFeatureDir("Todos");

        await new RemoveFeatureService("Todos", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldNotContain("@/features/todos/");
        indexRoute.ShouldNotContain("TodoList");
        indexRoute.ShouldNotContain("{/* dostar:feature:");
        indexRoute.ShouldContain("return <></>;");
    }

    [Fact]
    public async Task RemoveAsync_WithOtherFeatureInRoute_LeavesOtherFeatureSentinelIntact()
    {
        _repo.CreateFeatureDir("Todos");
        await File.WriteAllTextAsync(_repo.IndexRoutePath, """
            import type { JSX } from 'react';
            import { createFileRoute } from '@tanstack/react-router';
            import { TodoList } from '@/features/todos/components/TodoList';
            import { BillingList } from '@/features/billing/components/BillingList';

            export const Route = createFileRoute('/')({
                component: IndexPage,
            });

            function IndexPage(): JSX.Element {
                return (
                    <div className="mx-auto max-w-lg space-y-6">
                        {/* dostar:feature:todos:start */}
                        <TodoList />
                        {/* dostar:feature:todos:end */}
                        {/* dostar:feature:billing:start */}
                        <BillingList />
                        {/* dostar:feature:billing:end */}
                    </div>
                );
            }
            """);

        await new RemoveFeatureService("Todos", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldNotContain("@/features/todos/");
        indexRoute.ShouldNotContain("TodoList");
        indexRoute.ShouldNotContain("{/* dostar:feature:todos:");
        indexRoute.ShouldContain("@/features/billing/");
        indexRoute.ShouldContain("BillingList");
        indexRoute.ShouldContain("{/* dostar:feature:billing:start */}");
    }

    [Fact]
    public async Task RemoveAsync_DryRun_LeavesIndexRouteIntact()
    {
        _repo.CreateFeatureDir("Todos");

        await new RemoveFeatureService("Todos", dryRun: true, yes: true, _repo.RepoRoot).RemoveAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldContain("{/* dostar:feature:todos:start */}");
    }

    [Fact]
    public async Task RemoveAsync_FeatureNotInRoute_LeavesIndexRouteUnchanged()
    {
        _repo.CreateFeatureDir("Billing");
        var originalContent = await File.ReadAllTextAsync(_repo.IndexRoutePath);

        await new RemoveFeatureService("Billing", dryRun: false, yes: true, _repo.RepoRoot).RemoveAsync();

        var indexRoute = await File.ReadAllTextAsync(_repo.IndexRoutePath);
        indexRoute.ShouldBe(originalContent);
    }

    public void Dispose()
    {
        _repo.Dispose();
        GC.SuppressFinalize(this);
    }
}
