namespace Dostar.Cli;

internal static class TemplateRenderer
{
    internal static async Task RenderAsync(string templateName, object model, string outputPath)
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
