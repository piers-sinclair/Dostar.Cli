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

    private static Task<int> HandleAsync(string name, bool dryRun, bool yes) =>
        new RemoveModuleService(name, dryRun, yes).RemoveAsync();
}
