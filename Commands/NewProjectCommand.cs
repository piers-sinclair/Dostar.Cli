namespace Dostar.Cli;

internal static class NewProjectCommand
{
    internal static Command Build()
    {
        var nameArg = new Argument<string>("ProjectName")
        {
            Description = "PascalCase name for the new project (e.g. MyStartup)"
        };

        var outputOption = new Option<string?>("--output")
        {
            Description = "Destination directory (defaults to ./<ProjectName>)"
        };
        outputOption.Aliases.Add("-o");

        var ownerOption = new Option<string?>("--owner")
        {
            Description = "GitHub organisation or username that will own the repo (e.g. acme-corp). Replaces placeholder URLs in README badges, Bicep parameters, and dependabot config."
        };

        var command = new Command("new-project", "Bootstrap a new project from the Dostar template");
        command.Arguments.Add(nameArg);
        command.Options.Add(outputOption);
        command.Options.Add(ownerOption);

        command.SetAction((parseResult, _) => HandleAsync(
            parseResult.GetValue(nameArg)!,
            parseResult.GetValue(outputOption),
            parseResult.GetValue(ownerOption)));

        return command;
    }

    private static async Task<int> HandleAsync(string projectName, string? output, string? owner)
    {
        if (!projectName.IsPascalCase())
        {
            Console.Error.WriteLine($"Error: Project name '{projectName}' is not valid PascalCase.");
            Console.Error.WriteLine("The name must start with an uppercase letter and contain only letters and digits.");
            Console.Error.WriteLine("Examples: MyStartup, AcmeCorp, ProjectAlpha");
            return 1;
        }

        try
        {
            var outputDir = await new ProjectService(projectName, output, owner).CreateAsync();

            Console.WriteLine();
            Console.WriteLine($"Project '{projectName}' created successfully at '{outputDir}'.");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine($"  cd {projectName}");
            Console.WriteLine("  git init && git add . && git commit -m \"Initial commit\"");
            Console.WriteLine("  dotnet build");
            Console.WriteLine("  cd frontend && pnpm dev");

            if (owner is null)
            {
                Console.WriteLine();
                Console.WriteLine("Note: GitHub URLs were not updated (--owner was not provided).");
                Console.WriteLine("      Search for '__GITHUB_ORG__' across the project and replace with your");
                Console.WriteLine("      GitHub organisation or username before pushing.");
            }

            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
