namespace Dostar.Cli;

internal static class RemoveModuleCommand
{
    internal static Command Build()
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "The name of the module to remove (e.g. Billing)"
        };
        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Print what would be deleted without making any changes"
        };
        var yesOption = new Option<bool>("--yes")
        {
            Description = "Skip confirmation prompt"
        };
        yesOption.Aliases.Add("-y");

        var command = new Command("remove-module", "Remove a module and all associated projects from the solution");
        command.Arguments.Add(nameArg);
        command.Options.Add(dryRunOption);
        command.Options.Add(yesOption);

        command.SetAction((parseResult, _) => HandleAsync(
            parseResult.GetValue(nameArg)!,
            dryRun: parseResult.GetValue(dryRunOption),
            yes: parseResult.GetValue(yesOption)));

        return command;
    }

    private static async Task<int> HandleAsync(string name, bool dryRun, bool yes)
    {
        if (!name.IsPascalCase())
        {
            Console.Error.WriteLine($"Error: Module name '{name}' is not valid PascalCase.");
            Console.Error.WriteLine("The name must start with an uppercase letter and contain only letters and digits.");
            Console.Error.WriteLine("Examples: Billing, UserManagement, OrderProcessing");
            return 1;
        }

        return await new RemoveModuleService(name, dryRun, yes).RemoveAsync();
    }
}
