namespace Dostar.Cli.Tests.Unit;

public class NewProjectNextStepsTests
{
    [Fact]
    public void BuildNextSteps_WhenGhAvailable_UsesGhRepoCreate()
    {
        var result = NewProjectCommand.BuildNextSteps("Six", "moon-saber", ghAvailable: true);

        result.ShouldContain("gh repo create Six --private --source=. --remote=origin --push");
    }

    [Fact]
    public void BuildNextSteps_WhenGhAvailable_DoesNotContainManualGitRemote()
    {
        var result = NewProjectCommand.BuildNextSteps("Six", "moon-saber", ghAvailable: true);

        result.ShouldNotContain("git remote add origin");
    }

    [Fact]
    public void BuildNextSteps_WhenGhNotAvailable_ContainsGitHubNewUrl()
    {
        var result = NewProjectCommand.BuildNextSteps("Six", "moon-saber", ghAvailable: false);

        result.ShouldContain("https://github.com/new");
    }

    [Fact]
    public void BuildNextSteps_WhenGhNotAvailable_ContainsManualGitRemote()
    {
        var result = NewProjectCommand.BuildNextSteps("Six", "moon-saber", ghAvailable: false);

        result.ShouldContain("git remote add origin https://github.com/moon-saber/Six.git");
        result.ShouldContain("git push -u origin main");
    }

    [Fact]
    public void BuildNextSteps_WhenGhNotAvailable_DoesNotUseGhRepoCreate()
    {
        var result = NewProjectCommand.BuildNextSteps("Six", "moon-saber", ghAvailable: false);

        result.ShouldNotContain("gh repo create");
    }

    [Fact]
    public void BuildNextSteps_WhenGhAvailable_InterpolatesProjectName()
    {
        var result = NewProjectCommand.BuildNextSteps("MyStartup", "acme-corp", ghAvailable: true);

        result.ShouldContain("cd MyStartup");
        result.ShouldContain("gh repo create MyStartup");
    }

    [Fact]
    public void BuildNextSteps_WhenGhNotAvailable_InterpolatesProjectName()
    {
        var result = NewProjectCommand.BuildNextSteps("MyStartup", "acme-corp", ghAvailable: false);

        result.ShouldContain("'MyStartup'");
        result.ShouldContain("MyStartup.git");
    }

    [Fact]
    public void BuildNextSteps_WhenGhNotAvailable_InterpolatesOwnerInRepoUrl()
    {
        var result = NewProjectCommand.BuildNextSteps("MyStartup", "acme-corp", ghAvailable: false);

        result.ShouldContain("https://github.com/acme-corp/MyStartup.git");
    }

    [Fact]
    public void BuildNextSteps_BothPaths_ContainCodeDot()
    {
        var withGh = NewProjectCommand.BuildNextSteps("MyStartup", "acme-corp", ghAvailable: true);
        var withoutGh = NewProjectCommand.BuildNextSteps("MyStartup", "acme-corp", ghAvailable: false);

        withGh.ShouldContain("code .");
        withoutGh.ShouldContain("code .");
    }
}
