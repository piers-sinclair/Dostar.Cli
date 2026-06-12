namespace Dostar.Cli;

internal static class AddFeatureCommand
{
    internal static Command Build()
    {
        var nameArg = new Argument<string>("name")
        {
            Description = "PascalCase name for the new feature (e.g. Billing)"
        };

        var command = new Command("add-feature", "Scaffold a new frontend feature folder with components, hooks, and mocks");
        command.Arguments.Add(nameArg);
        command.SetAction((parseResult, _) => HandleAsync(parseResult.GetValue(nameArg)!));

        return command;
    }

    private static async Task<int> HandleAsync(string name)
    {
        if (!name.IsPascalCase())
        {
            Console.Error.WriteLine($"Error: Feature name '{name}' is not valid PascalCase.");
            Console.Error.WriteLine("The name must start with an uppercase letter and contain only letters and digits.");
            Console.Error.WriteLine("Examples: Billing, UserManagement, OrderProcessing");
            return 1;
        }

        Console.WriteLine($"Scaffolding feature '{name}'...");
        if (!await new AddFeatureService(name).AddAsync())
            return 0;

        Console.WriteLine($"Feature '{name}' scaffolded successfully.");
        Console.WriteLine(" Next steps:");
        Console.WriteLine($" - Replace 'unknown[]' in hooks/use{name}.ts with your real API response type");
        Console.WriteLine($" - Add typed test fixtures to mocks/handlers.ts (see todos feature for an example)");
        Console.WriteLine(" - Implement your components");
        return 0;
    }
}
