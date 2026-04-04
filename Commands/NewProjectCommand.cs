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
            await new ProjectService(projectName, output).CreateAsync();
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
