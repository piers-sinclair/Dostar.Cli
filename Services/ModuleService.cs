namespace Dostar.Cli;

internal sealed class ModuleService(string name, bool endpoints)
{
    private readonly RepoRoot _root = RepoRoot.Find();
    private string Prefix     => Path.GetFileNameWithoutExtension(_root.SlnxPath);
    private string ModulesDir => Path.Combine(_root.Root, "backend", "Modules", name);

    internal async Task<bool> AddAsync()
    {
        if (Directory.Exists(ModulesDir))
        {
            Console.WriteLine($"Module '{name}' already exists at {ModulesDir}. Nothing to do.");
            return false;
        }

        var apiCsprojPath = Path.Combine(_root.Root, "backend", $"{Prefix}.Api", $"{Prefix}.Api.csproj");
        var implCsprojPath = Path.Combine(ModulesDir, $"{Prefix}.{name}.Implementation", $"{Prefix}.{name}.Implementation.csproj");

        await GenerateProjectFilesAsync(apiCsprojPath);
        await SolutionCli.AddProjectsAsync(_root.SlnxPath, "/backend/", ModuleProjectPaths(), _root.Root);
        await SolutionCli.AddReferenceAsync(apiCsprojPath, implCsprojPath, _root.Root);
        await RegisterModuleAsync();

        return true;
    }

    private string[] ModuleProjectPaths() =>
    [
        $"backend/Modules/{name}/{Prefix}.{name}.Contracts/{Prefix}.{name}.Contracts.csproj",
        $"backend/Modules/{name}/{Prefix}.{name}.Implementation/{Prefix}.{name}.Implementation.csproj",
        $"backend/Modules/{name}/{Prefix}.{name}.UnitTests/{Prefix}.{name}.UnitTests.csproj",
        $"backend/Modules/{name}/{Prefix}.{name}.IntegrationTests/{Prefix}.{name}.IntegrationTests.csproj",
    ];

    private async Task GenerateProjectFilesAsync(string apiCsprojPath)
    {
        var targetFramework     = await CsprojReader.ReadTargetFrameworkAsync(apiCsprojPath);
        var model               = new { name, prefix = Prefix, endpoints, targetFramework };
        var contractsDir        = Path.Combine(ModulesDir, $"{Prefix}.{name}.Contracts");
        var implDir             = Path.Combine(ModulesDir, $"{Prefix}.{name}.Implementation");
        var unitTestsDir        = Path.Combine(ModulesDir, $"{Prefix}.{name}.UnitTests");
        var integrationTestsDir = Path.Combine(ModulesDir, $"{Prefix}.{name}.IntegrationTests");

        Directory.CreateDirectory(contractsDir);
        Directory.CreateDirectory(implDir);
        Directory.CreateDirectory(unitTestsDir);
        Directory.CreateDirectory(integrationTestsDir);

        await Task.WhenAll(
            TemplateRenderer.RenderAsync("Contracts.csproj.scriban",                model, Path.Combine(contractsDir,         $"{Prefix}.{name}.Contracts.csproj")),
            TemplateRenderer.RenderAsync("Implementation.csproj.scriban",           model, Path.Combine(implDir,              $"{Prefix}.{name}.Implementation.csproj")),
            TemplateRenderer.RenderAsync("Module.cs.scriban",                       model, Path.Combine(implDir,              $"{name}Module.cs")),
            TemplateRenderer.RenderAsync("ImplementationGlobalUsings.cs.scriban",   model, Path.Combine(implDir,              "GlobalUsings.cs")),
            TemplateRenderer.RenderAsync("UnitTests.csproj.scriban",                model, Path.Combine(unitTestsDir,         $"{Prefix}.{name}.UnitTests.csproj")),
            TemplateRenderer.RenderAsync("UnitTestsGlobalUsings.cs.scriban",        model, Path.Combine(unitTestsDir,         "GlobalUsings.cs")),
            TemplateRenderer.RenderAsync("UnitTestsClass.cs.scriban",               model, Path.Combine(unitTestsDir,         $"{name}ModuleTests.cs")),
            TemplateRenderer.RenderAsync("IntegrationTests.csproj.scriban",         model, Path.Combine(integrationTestsDir,  $"{Prefix}.{name}.IntegrationTests.csproj")),
            TemplateRenderer.RenderAsync("IntegrationTestsGlobalUsings.cs.scriban", model, Path.Combine(integrationTestsDir,  "GlobalUsings.cs")),
            TemplateRenderer.RenderAsync("IntegrationTestsClass.cs.scriban",        model, Path.Combine(integrationTestsDir,  $"{name}ModuleIntegrationTests.cs")),
            TemplateRenderer.RenderAsync("ApiFactory.cs.scriban",                   model, Path.Combine(integrationTestsDir,  "ApiFactory.cs")));

        Console.WriteLine("  Generated project files.");
    }


    private async Task RegisterModuleAsync()
    {
        var programCsPath = Path.Combine(_root.Root, "backend", $"{Prefix}.Api", "Program.cs");
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

        content = EnsureUsingStatement(content);
        content = InsertModuleIntoArray(content);

        await File.WriteAllTextAsync(programCsPath, content);
        Console.WriteLine($"  Registered {name}Module in Program.cs.");
    }

    private string EnsureUsingStatement(string content)
    {
        var usingStatement = $"using {Prefix}.{name}.Implementation;";
        if (content.Contains(usingStatement))
            return content;

        return $"{usingStatement}\n{content}";
    }

    private string InsertModuleIntoArray(string content)
    {
        var moduleArrayPattern = "IModule[] modules =\n[";
        var moduleArrayIndex = content.IndexOf(moduleArrayPattern, StringComparison.Ordinal);
        if (moduleArrayIndex < 0)
        {
            Console.Error.WriteLine("Warning: Could not find 'IModule[] modules' array in Program.cs. Please register the module manually.");
            return content;
        }

        var closingIndex = content.IndexOf("];", moduleArrayIndex, StringComparison.Ordinal);
        if (closingIndex < 0)
        {
            Console.Error.WriteLine("Warning: Could not find end of modules array in Program.cs. Please register the module manually.");
            return content;
        }

        return content.Insert(closingIndex, $"    new {name}Module(),\n");
    }
}
