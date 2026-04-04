namespace Dostar.Cli.Commands;

internal static class AddModuleCommand
{
    private static readonly Regex PascalCaseRegex = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

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
        if (!PascalCaseRegex.IsMatch(name))
        {
            Console.Error.WriteLine($"Error: Module name '{name}' is not valid PascalCase.");
            Console.Error.WriteLine("The name must start with an uppercase letter and contain only letters and digits.");
            Console.Error.WriteLine("Examples: Billing, UserManagement, OrderProcessing");
            return 1;
        }

        var root = FindRepoRoot();
        if (root is null)
        {
            Console.Error.WriteLine("Error: could not find repo root (no .slnx file found).");
            return 1;
        }

        var (repoRoot, slnxPath) = root.Value;
        var prefix = Path.GetFileNameWithoutExtension(slnxPath);
        var modulesDir = Path.Combine(repoRoot, "backend", "Modules", name);

        if (Directory.Exists(modulesDir))
        {
            Console.WriteLine($"Module '{name}' already exists at {modulesDir}. Nothing to do.");
            return 0;
        }

        Console.WriteLine($"Scaffolding module '{name}'...");

        var apiCsprojPath = Path.Combine(repoRoot, "backend", $"{prefix}.Api", $"{prefix}.Api.csproj");
        var targetFramework = await ReadTargetFrameworkAsync(apiCsprojPath);
        var model = new { name, prefix, endpoints, targetFramework };

        var contractsDir = Path.Combine(modulesDir, $"{prefix}.{name}.Contracts");
        var implDir = Path.Combine(modulesDir, $"{prefix}.{name}.Implementation");
        var unitTestsDir = Path.Combine(modulesDir, $"{prefix}.{name}.UnitTests");
        var integrationTestsDir = Path.Combine(modulesDir, $"{prefix}.{name}.IntegrationTests");

        Directory.CreateDirectory(contractsDir);
        Directory.CreateDirectory(implDir);
        Directory.CreateDirectory(unitTestsDir);
        Directory.CreateDirectory(integrationTestsDir);

        await Task.WhenAll(
            RenderTemplateAsync("Contracts.csproj.scriban", model,
                Path.Combine(contractsDir, $"{prefix}.{name}.Contracts.csproj")),
            RenderTemplateAsync("Implementation.csproj.scriban", model,
                Path.Combine(implDir, $"{prefix}.{name}.Implementation.csproj")),
            RenderTemplateAsync("Module.cs.scriban", model,
                Path.Combine(implDir, $"{name}Module.cs")),
            RenderTemplateAsync("ImplementationGlobalUsings.cs.scriban", model,
                Path.Combine(implDir, "GlobalUsings.cs")),
            RenderTemplateAsync("UnitTests.csproj.scriban", model,
                Path.Combine(unitTestsDir, $"{prefix}.{name}.UnitTests.csproj")),
            RenderTemplateAsync("UnitTestsGlobalUsings.cs.scriban", model,
                Path.Combine(unitTestsDir, "GlobalUsings.cs")),
            RenderTemplateAsync("UnitTestsClass.cs.scriban", model,
                Path.Combine(unitTestsDir, $"{name}ModuleTests.cs")),
            RenderTemplateAsync("IntegrationTests.csproj.scriban", model,
                Path.Combine(integrationTestsDir, $"{prefix}.{name}.IntegrationTests.csproj")),
            RenderTemplateAsync("IntegrationTestsGlobalUsings.cs.scriban", model,
                Path.Combine(integrationTestsDir, "GlobalUsings.cs")),
            RenderTemplateAsync("IntegrationTestsClass.cs.scriban", model,
                Path.Combine(integrationTestsDir, $"{name}ModuleIntegrationTests.cs")),
            RenderTemplateAsync("ApiFactory.cs.scriban", model,
                Path.Combine(integrationTestsDir, "ApiFactory.cs")));

        Console.WriteLine("  Generated project files.");

        await AddProjectsToSolutionAsync(slnxPath, name, prefix, repoRoot);

        var implCsproj = Path.Combine(modulesDir, $"{prefix}.{name}.Implementation", $"{prefix}.{name}.Implementation.csproj");
        var addRefResult = await RunProcessAsync("dotnet", $"add \"{apiCsprojPath}\" reference \"{implCsproj}\"", repoRoot);
        if (addRefResult != 0)
            Console.Error.WriteLine($"Warning: 'dotnet add reference' exited with code {addRefResult}.");
        else
            Console.WriteLine($"  Added reference from {prefix}.Api to {prefix}.{name}.Implementation.");

        var programCsPath = Path.Combine(repoRoot, "backend", $"{prefix}.Api", "Program.cs");
        await AddModuleRegistrationAsync(programCsPath, name, prefix);

        Console.WriteLine($"Module '{name}' scaffolded successfully.");
        Console.WriteLine($"  Location: {modulesDir}");
        Console.WriteLine("  Next steps:");
        Console.WriteLine("    - Run: dotnet build");
        Console.WriteLine($"    - Add EF Core migrations if needed");
        return 0;
    }

    private static async Task RenderTemplateAsync(string templateName, object model, string outputPath)
    {
        var templateContent = LoadEmbeddedTemplate(templateName);
        var template = Template.Parse(templateContent);
        var result = await template.RenderAsync(model);
        await File.WriteAllTextAsync(outputPath, result);
    }

    private static string LoadEmbeddedTemplate(string templateName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"Dostar.Cli.Templates.{templateName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded template '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static async Task AddProjectsToSolutionAsync(string slnxPath, string name, string prefix, string repoRoot)
    {
        var projects = new[]
        {
            $"backend/Modules/{name}/{prefix}.{name}.Contracts/{prefix}.{name}.Contracts.csproj",
            $"backend/Modules/{name}/{prefix}.{name}.Implementation/{prefix}.{name}.Implementation.csproj",
            $"backend/Modules/{name}/{prefix}.{name}.UnitTests/{prefix}.{name}.UnitTests.csproj",
            $"backend/Modules/{name}/{prefix}.{name}.IntegrationTests/{prefix}.{name}.IntegrationTests.csproj",
        };

        foreach (var project in projects)
        {
            var result = await RunProcessAsync(
                "dotnet",
                $"sln \"{slnxPath}\" add --solution-folder \"/backend/\" \"{project}\"",
                repoRoot);

            if (result != 0)
                Console.Error.WriteLine($"Warning: 'dotnet sln add' exited with code {result} for {project}");
        }

        Console.WriteLine($"  Added {projects.Length} projects to {Path.GetFileName(slnxPath)}.");
    }

    private static async Task AddModuleRegistrationAsync(string programCsPath, string name, string prefix)
    {
        if (!File.Exists(programCsPath))
        {
            Console.Error.WriteLine($"Warning: Program.cs not found at {programCsPath}");
            return;
        }

        var content = (await File.ReadAllTextAsync(programCsPath)).Replace("\r\n", "\n");

        if (content.Contains($"new {name}Module()"))
        {
            Console.WriteLine($"  {name}Module already registered in Program.cs.");
            return;
        }

        var usingStatement = $"using {prefix}.{name}.Implementation;";
        if (!content.Contains(usingStatement))
        {
            var lastProjectUsing = content.LastIndexOf($"using {prefix}.", StringComparison.Ordinal);
            if (lastProjectUsing >= 0)
            {
                var endOfLine = content.IndexOf('\n', lastProjectUsing);
                content = content.Insert(endOfLine + 1, $"{usingStatement}\n");
            }
            else
            {
                content = $"{usingStatement}\n{content}";
            }
        }

        var moduleArrayPattern = "IModule[] modules =\n[";
        var moduleArrayIndex = content.IndexOf(moduleArrayPattern, StringComparison.Ordinal);
        if (moduleArrayIndex < 0)
        {
            Console.Error.WriteLine("Warning: Could not find 'IModule[] modules' array in Program.cs. Please register the module manually.");
            await File.WriteAllTextAsync(programCsPath, content);
            return;
        }

        var closingIndex = content.IndexOf("];", moduleArrayIndex, StringComparison.Ordinal);
        if (closingIndex < 0)
        {
            Console.Error.WriteLine("Warning: Could not find end of modules array in Program.cs. Please register the module manually.");
            await File.WriteAllTextAsync(programCsPath, content);
            return;
        }

        content = content.Insert(closingIndex, $"    new {name}Module(),\n");
        await File.WriteAllTextAsync(programCsPath, content);
        Console.WriteLine($"  Registered {name}Module in Program.cs.");
    }

    private static async Task<string> ReadTargetFrameworkAsync(string apiCsprojPath)
    {
        if (!File.Exists(apiCsprojPath))
        {
            Console.Error.WriteLine($"Warning: could not find {apiCsprojPath} to detect target framework; defaulting to net10.0.");
            return "net10.0";
        }

        var xml = XDocument.Parse(await File.ReadAllTextAsync(apiCsprojPath));
        var value = xml.Descendants("TargetFramework").FirstOrDefault()?.Value;
        if (value is null)
        {
            Console.Error.WriteLine($"Warning: <TargetFramework> not found in {apiCsprojPath}; defaulting to net10.0.");
            return "net10.0";
        }

        return value;
    }

    private static (string Root, string SlnxPath)? FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var slnx = dir.GetFiles("*.slnx").FirstOrDefault();
            if (slnx is not null)
                return (dir.FullName, slnx.FullName);
            dir = dir.Parent;
        }

        return null;
    }

    private static async Task<int> RunProcessAsync(string fileName, string arguments, string workingDirectory)
    {
        using var process = new System.Diagnostics.Process();
        process.StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                Console.Error.WriteLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}