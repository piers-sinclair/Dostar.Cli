namespace Dostar.Cli.Tests.Unit;

public class ProjectNameSubstitutorTests
{
    private static string Sub(string input) =>
        ProjectNameSubstitutor.Substitute(input, "MyApp", "myapp", "my-org");

    [Fact]
    public void Substitute_ProjectName_ReplacesDostarWithProjectName()
    {
        Sub("namespace Dostar.Todos").ShouldBe("namespace MyApp.Todos");
    }

    [Fact]
    public void Substitute_ProjectNameLower_ReplacesLowercaseDostar()
    {
        Sub("POSTGRES_DB: dostar").ShouldBe("POSTGRES_DB: myapp");
    }

    [Fact]
    public void Substitute_GithubOrg_ReplacesOrgAndProjectNameInRepoUrl()
    {
        Sub("piers-sinclair/Dostar").ShouldBe("my-org/MyApp");
    }

    [Fact]
    public void Substitute_CliToolName_PreservesDostarCli()
    {
        Sub("dotnet tool install -g Dostar.Cli").ShouldBe("dotnet tool install -g Dostar.Cli");
    }

    [Fact]
    public void Substitute_CliRepoRef_PreservesPiersSinclairDostarCli()
    {
        Sub("[Dostar.Cli](https://github.com/piers-sinclair/Dostar.Cli)").ShouldBe("[Dostar.Cli](https://github.com/piers-sinclair/Dostar.Cli)");
    }

    [Fact]
    public void Substitute_CliInlineCode_PreservesBacktickDostar()
    {
        Sub("The `dostar` CLI tool").ShouldBe("The `dostar` CLI tool");
    }

    [Fact]
    public void Substitute_CliNewProject_PreservesCommand()
    {
        Sub("dostar new-project MyStartup").ShouldBe("dostar new-project MyStartup");
    }

    [Fact]
    public void Substitute_CliAddModule_PreservesCommand()
    {
        Sub("dostar add-module Products").ShouldBe("dostar add-module Products");
    }

    [Fact]
    public void Substitute_CliRemoveModule_PreservesCommand()
    {
        Sub("dostar remove-module Products").ShouldBe("dostar remove-module Products");
    }

    [Fact]
    public void Substitute_BicepParamWorkload_ReplacesWithProjectNameLower()
    {
        Sub("param workload = 'dostar'").ShouldBe("param workload = 'myapp'");
    }

    [Fact]
    public void Substitute_NoSubstituteAnnotation_LeavesEntireLineUnchanged()
    {
        const string input = """
            check_tool "dostar"  "dotnet tool install -g Dostar.Cli"  dostar --version  # @no-substitute
            POSTGRES_DB: dostar
            """;

        var result = Sub(input);

        result.ShouldContain("check_tool \"dostar\"  \"dotnet tool install -g Dostar.Cli\"  dostar --version  # @no-substitute");
        result.ShouldContain("POSTGRES_DB: myapp");
    }

    [Fact]
    public void Substitute_MixedContent_SubstitutesProjectNameAndPreservesCli()
    {
        const string input = """
            # MyProject setup

            Install: dotnet tool install -g Dostar.Cli
            Run: dostar new-project MyStartup

            namespace Dostar.Todos;
            POSTGRES_DB: dostar
            """;

        var result = Sub(input);

        result.ShouldContain("Dostar.Cli");
        result.ShouldContain("dostar new-project");
        result.ShouldContain("namespace MyApp.Todos");
        result.ShouldContain("POSTGRES_DB: myapp");
    }

    [Fact]
    public void Substitute_ContributingIssueLink_ReplacesOrgAndProjectName()
    {
        Sub("https://github.com/piers-sinclair/Dostar/issues")
            .ShouldBe("https://github.com/my-org/MyApp/issues");
    }

    [Fact]
    public void Substitute_DostarCliInReadmeTable_PreservesFullReference()
    {
        const string line = "| CLI tool | `dostar` — .NET global tool in [piers-sinclair/Dostar.Cli](https://github.com/piers-sinclair/Dostar.Cli) |";
        var result = Sub(line);
        result.ShouldContain("piers-sinclair/Dostar.Cli");
        result.ShouldContain("Dostar.Cli");
        result.ShouldNotContain("MyApp.Cli");
        result.ShouldNotContain("my-org/Dostar.Cli");
    }

    [Fact]
    public void Substitute_DocsMarkdownNamespace_ReplacesProjectPrefix()
    {
        Sub("- `Dostar.SharedKernel`, `Dostar.Todos.Contracts`, `Dostar.Api`")
            .ShouldBe("- `MyApp.SharedKernel`, `MyApp.Todos.Contracts`, `MyApp.Api`");
    }

    [Fact]
    public void Substitute_DostarNewProjectInChecklistNote_PreservesCommand()
    {
        Sub("_already done if you ran `dostar new-project`_")
            .ShouldBe("_already done if you ran `dostar new-project`_");
    }
}
