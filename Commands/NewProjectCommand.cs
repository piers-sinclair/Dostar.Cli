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

        var command = new Command("new-project", "Bootstrap a new project from the Dostar template");
        command.Arguments.Add(nameArg);
        command.Options.Add(outputOption);

        command.SetAction((parseResult, _) => HandleAsync(
            parseResult.GetValue(nameArg)!,
            parseResult.GetValue(outputOption)));

        return command;
    }

    private static async Task<int> HandleAsync(string projectName, string? output)
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
            var outputDir = await new ProjectService(projectName, output).CreateAsync();

            Console.WriteLine();
            Console.WriteLine($"Project '{projectName}' created successfully at '{outputDir}'.");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine($"  cd {projectName}");
            Console.WriteLine("  git init && git add . && git commit -m \"Initial commit\"");
            Console.WriteLine("  dotnet build");
            Console.WriteLine("  cd frontend && pnpm dev");
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
