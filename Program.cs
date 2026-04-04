var rootCommand = new RootCommand("dostar — Dostar modular monolith CLI");

rootCommand.Subcommands.Add(NewProjectCommand.Build());
rootCommand.Subcommands.Add(AddModuleCommand.Build());

try
{
    return await rootCommand.Parse(args).InvokeAsync();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
