namespace Dostar.Cli;

internal static class AddModuleCommand
{
    internal static Command Build()
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "PascalCase name for the new module (e.g. Billing)"
        };

        var noEndpointsOption = new Option<bool>("--no-endpoints")
        {
            Description = "Scaffold as IModule (no HTTP endpoints) instead of IEndpointModule"
        };

        var command = new Command("add-module", "Scaffold a new feature module with Contracts, Implementation, UnitTests, and IntegrationTests projects");
        command.Arguments.Add(nameArg);
        command.Options.Add(noEndpointsOption);

        command.SetAction((parseResult, _) => HandleAsync(
            parseResult.GetValue(nameArg)!,
            endpoints: !parseResult.GetValue(noEndpointsOption)));

        return command;
    }

    private static async Task<int> HandleAsync(string name, bool endpoints = true)
    {
        if (!name.IsPascalCase())
        {
            Console.Error.WriteLine($"Error: Module name '{name}' is not valid PascalCase.");
            Console.Error.WriteLine("The name must start with an uppercase letter and contain only letters and digits.");
            Console.Error.WriteLine("Examples: Billing, UserManagement, OrderProcessing");
            return 1;
        }

        Console.WriteLine($"Scaffolding module '{name}'...");
        if (!await new AddModuleService(name, endpoints).AddAsync())
            return 0;

        Console.WriteLine($"Module '{name}' scaffolded successfully.");
        Console.WriteLine("  Next steps:");
        Console.WriteLine("    - Run: dotnet build");
        Console.WriteLine("    - Add EF Core migrations if needed");
        return 0;
    }
}
