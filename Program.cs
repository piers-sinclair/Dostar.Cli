var rootCommand = new RootCommand("dostar — Dostar modular monolith CLI");

rootCommand.Subcommands.Add(NewProjectCommand.Build());
rootCommand.Subcommands.Add(AddModuleCommand.Build());
rootCommand.Subcommands.Add(RemoveModuleCommand.Build());
rootCommand.Subcommands.Add(RemoveFeatureCommand.Build());
rootCommand.Subcommands.Add(AddFeatureCommand.Build());

try
{
    return await rootCommand.Parse(args).InvokeAsync();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
