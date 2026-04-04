namespace Dostar.Cli;

internal sealed class ModuleService(string name, bool endpoints)
{
    private readonly RepoRoot _root   = FindRepoRoot();
    private string Prefix             => Path.GetFileNameWithoutExtension(_root.SlnxPath);
    private string ModulesDir         => Path.Combine(_root.Root, "backend", "Modules", name);
    private string ApiCsprojPath      => Path.Combine(_root.Root, "backend", $"{Prefix}.Api", $"{Prefix}.Api.csproj");
    private string ProgramCsPath      => Path.Combine(_root.Root, "backend", $"{Prefix}.Api", "Program.cs");

    internal async Task<bool> AddAsync()
    {
        if (Directory.Exists(ModulesDir))
        {
            Console.WriteLine($"Module '{name}' already exists at {ModulesDir}. Nothing to do.");
            return false;
        }

        var targetFramework = await ReadTargetFrameworkAsync();
        var model = new { name, prefix = Prefix, endpoints, targetFramework };

        await ScaffoldProjectFilesAsync(model);
        await AddProjectsToSolutionAsync();
        await AddApiReferenceAsync();
        await RegisterModuleAsync();

        return true;
    }

    private async Task ScaffoldProjectFilesAsync(object model)
    {
        var contractsDir        = Path.Combine(ModulesDir, $"{Prefix}.{name}.Contracts");
        var implDir             = Path.Combine(ModulesDir, $"{Prefix}.{name}.Implementation");
        var unitTestsDir        = Path.Combine(ModulesDir, $"{Prefix}.{name}.UnitTests");
        var integrationTestsDir = Path.Combine(ModulesDir, $"{Prefix}.{name}.IntegrationTests");

        Directory.CreateDirectory(contractsDir);
        Directory.CreateDirectory(implDir);
        Directory.CreateDirectory(unitTestsDir);
        Directory.CreateDirectory(integrationTestsDir);

        await Task.WhenAll(
            RenderTemplateAsync("Contracts.csproj.scriban",                model, Path.Combine(contractsDir,         $"{Prefix}.{name}.Contracts.csproj")),
            RenderTemplateAsync("Implementation.csproj.scriban",           model, Path.Combine(implDir,              $"{Prefix}.{name}.Implementation.csproj")),
            RenderTemplateAsync("Module.cs.scriban",                       model, Path.Combine(implDir,              $"{name}Module.cs")),
            RenderTemplateAsync("ImplementationGlobalUsings.cs.scriban",   model, Path.Combine(implDir,              "GlobalUsings.cs")),
            RenderTemplateAsync("UnitTests.csproj.scriban",                model, Path.Combine(unitTestsDir,         $"{Prefix}.{name}.UnitTests.csproj")),
            RenderTemplateAsync("UnitTestsGlobalUsings.cs.scriban",        model, Path.Combine(unitTestsDir,         "GlobalUsings.cs")),
            RenderTemplateAsync("UnitTestsClass.cs.scriban",               model, Path.Combine(unitTestsDir,         $"{name}ModuleTests.cs")),
            RenderTemplateAsync("IntegrationTests.csproj.scriban",         model, Path.Combine(integrationTestsDir,  $"{Prefix}.{name}.IntegrationTests.csproj")),
            RenderTemplateAsync("IntegrationTestsGlobalUsings.cs.scriban", model, Path.Combine(integrationTestsDir,  "GlobalUsings.cs")),
            RenderTemplateAsync("IntegrationTestsClass.cs.scriban",        model, Path.Combine(integrationTestsDir,  $"{name}ModuleIntegrationTests.cs")),
            RenderTemplateAsync("ApiFactory.cs.scriban",                   model, Path.Combine(integrationTestsDir,  "ApiFactory.cs")));

        Console.WriteLine("  Generated project files.");
    }

    private async Task AddProjectsToSolutionAsync()
    {
        var projects = new[]
        {
            $"backend/Modules/{name}/{Prefix}.{name}.Contracts/{Prefix}.{name}.Contracts.csproj",
            $"backend/Modules/{name}/{Prefix}.{name}.Implementation/{Prefix}.{name}.Implementation.csproj",
            $"backend/Modules/{name}/{Prefix}.{name}.UnitTests/{Prefix}.{name}.UnitTests.csproj",
            $"backend/Modules/{name}/{Prefix}.{name}.IntegrationTests/{Prefix}.{name}.IntegrationTests.csproj",
        };

        foreach (var project in projects)
        {
            var result = await ProcessRunner.RunAsync(
                "dotnet",
                ["sln", _root.SlnxPath, "add", "--solution-folder", "/backend/", project],
                _root.Root);

            if (result != 0)
                Console.Error.WriteLine($"Warning: 'dotnet sln add' exited with code {result} for {project}");
        }

        Console.WriteLine($"  Added {projects.Length} projects to {Path.GetFileName(_root.SlnxPath)}.");
    }

    private async Task AddApiReferenceAsync()
    {
        var implCsproj = Path.Combine(ModulesDir, $"{Prefix}.{name}.Implementation", $"{Prefix}.{name}.Implementation.csproj");
        var result = await ProcessRunner.RunAsync("dotnet", ["add", ApiCsprojPath, "reference", implCsproj], _root.Root);
        if (result != 0)
            Console.Error.WriteLine($"Warning: 'dotnet add reference' exited with code {result}.");
        else
            Console.WriteLine($"  Added reference from {Prefix}.Api to {Prefix}.{name}.Implementation.");
    }

    private async Task RegisterModuleAsync()
    {
        if (!File.Exists(ProgramCsPath))
        {
            Console.Error.WriteLine($"Warning: Program.cs not found at {ProgramCsPath}");
            return;
        }

        var content = (await File.ReadAllTextAsync(ProgramCsPath)).Replace("\r\n", "\n");

        if (content.Contains($"new {name}Module()"))
        {
            Console.WriteLine($"  {name}Module already registered in Program.cs.");
            return;
        }

        content = EnsureUsingStatement(content);
        content = InsertModuleIntoArray(content);

        await File.WriteAllTextAsync(ProgramCsPath, content);
        Console.WriteLine($"  Registered {name}Module in Program.cs.");
    }

    private string EnsureUsingStatement(string content)
    {
        var usingStatement = $"using {Prefix}.{name}.Implementation;";
        if (content.Contains(usingStatement))
            return content;

        var lastProjectUsing = content.LastIndexOf($"using {Prefix}.", StringComparison.Ordinal);
        if (lastProjectUsing >= 0)
        {
            var endOfLine = content.IndexOf('\n', lastProjectUsing);
            return content.Insert(endOfLine + 1, $"{usingStatement}\n");
        }

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

    private async Task<string> ReadTargetFrameworkAsync()
    {
        if (!File.Exists(ApiCsprojPath))
        {
            Console.Error.WriteLine($"Warning: could not find {ApiCsprojPath} to detect target framework; defaulting to net10.0.");
            return "net10.0";
        }

        var xml = XDocument.Parse(await File.ReadAllTextAsync(ApiCsprojPath));
        var value = xml.Descendants("TargetFramework").FirstOrDefault()?.Value;
        if (value is null)
        {
            Console.Error.WriteLine($"Warning: <TargetFramework> not found in {ApiCsprojPath}; defaulting to net10.0.");
            return "net10.0";
        }

        return value;
    }

    private static RepoRoot FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var slnx = dir.GetFiles("*.slnx").FirstOrDefault();
            if (slnx is not null)
                return new(dir.FullName, slnx.FullName);
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find repo root (no .slnx file found).");
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
}

internal sealed record RepoRoot(string Root, string SlnxPath);
