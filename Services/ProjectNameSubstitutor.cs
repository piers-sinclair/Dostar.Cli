namespace Dostar.Cli;

internal static class ProjectNameSubstitutor
{
    // Sentinel placeholders — \x01 is not a valid identifier character, so these can never appear
    // in source files naturally and will never collide with substituted content.
    private const string PCliRepo = "\x01CLI_REPO\x01";    // piers-sinclair/Dostar.Cli
    private const string PCliTool = "\x01CLI_TOOL\x01";    // Dostar.Cli (bare)
    private const string PCliInline = "\x01CLI_INL\x01";   // `dostar` (inline code span)
    private const string PCliNew = "\x01CLI_NEW\x01";      // dostar new-project
    private const string PCliAdd = "\x01CLI_ADD\x01";      // dostar add-module
    private const string PCliRemove = "\x01CLI_REM\x01";   // dostar remove-module
    private const string PCliAddF = "\x01CLI_ADDF\x01";    // dostar add-feature
    private const string PCliRemoveF = "\x01CLI_REMF\x01"; // dostar remove-feature
    private const string PCliFSent = "\x01CLI_FSNT\x01";   // dostar:feature: (sentinel comment prefix)
    private const string PCliJsxBare = "\x01CLI_JSXB\x01"; // <code ...>dostar</code> (bare CLI name in JSX)

    internal static string Substitute(string input, string projectName, string projectNameLower, string githubOrg)
    {
        if (!input.Contains("@no-substitute", StringComparison.Ordinal))
            return ApplySubstitutions(input, projectName, projectNameLower, githubOrg);

        // Lines annotated with @no-substitute are left unchanged (e.g. external tool references in shell scripts)
        var lines = input.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains("@no-substitute", StringComparison.Ordinal))
                lines[i] = ApplySubstitutions(lines[i], projectName, projectNameLower, githubOrg);
        }
        return string.Join('\n', lines);
    }

    private static string ApplySubstitutions(string input, string projectName, string projectNameLower, string githubOrg) =>
        input
            // Protect CLI tool references — longest/most-specific patterns first so shorter ones
            // don't partially match before the longer ones are replaced
            .Replace("piers-sinclair/Dostar.Cli", PCliRepo)
            .Replace("Dostar.Cli", PCliTool)
            .Replace("`dostar`", PCliInline)
            .Replace("dostar new-project", PCliNew)
            .Replace("dostar add-module", PCliAdd)
            .Replace("dostar remove-module", PCliRemove)
            .Replace("dostar add-feature", PCliAddF)
            .Replace("dostar remove-feature", PCliRemoveF)
            .Replace("dostar:feature:", PCliFSent)
            .Replace("<code className=\"text-xs\">dostar</code>", PCliJsxBare)
            // Apply project-name substitutions
            .Replace("piers-sinclair/Dostar", $"{githubOrg}/Dostar")
            .Replace("\"piers-sinclair\"", $"\"{githubOrg}\"")
            .Replace("Dostar", projectName, StringComparison.Ordinal)
            .Replace("dostar", projectNameLower, StringComparison.Ordinal)
            // Restore protected CLI references
            .Replace(PCliRepo, "piers-sinclair/Dostar.Cli")
            .Replace(PCliTool, "Dostar.Cli")
            .Replace(PCliInline, "`dostar`")
            .Replace(PCliNew, "dostar new-project")
            .Replace(PCliAdd, "dostar add-module")
            .Replace(PCliRemove, "dostar remove-module")
            .Replace(PCliAddF, "dostar add-feature")
            .Replace(PCliRemoveF, "dostar remove-feature")
            .Replace(PCliFSent, "dostar:feature:")
            .Replace(PCliJsxBare, "<code className=\"text-xs\">dostar</code>");
}
