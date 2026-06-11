namespace Dostar.Cli;

internal static class ProjectNameSubstitutor
{
    // Sentinel placeholders — \x01 is not a valid identifier character, so these can never appear
    // in source files naturally and will never collide with substituted content.
    private const string PCliRepo   = "\x01CLI_REPO\x01";   // piers-sinclair/Dostar.Cli
    private const string PCliTool   = "\x01CLI_TOOL\x01";   // Dostar.Cli (bare)
    private const string PCliInline = "\x01CLI_INL\x01";    // `dostar` (inline code span)
    private const string PCliNew    = "\x01CLI_NEW\x01";    // dostar new-project
    private const string PCliAdd    = "\x01CLI_ADD\x01";    // dostar add-module
    private const string PCliRemove = "\x01CLI_REM\x01";    // dostar remove-module

    internal static string Substitute(string input, string projectName, string projectNameLower, string githubOrg) =>
        input
            // Protect CLI tool references — longest/most-specific patterns first so shorter ones
            // don't partially match before the longer ones are replaced
            .Replace("piers-sinclair/Dostar.Cli", PCliRepo)
            .Replace("Dostar.Cli",                PCliTool)
            .Replace("`dostar`",                  PCliInline)
            .Replace("dostar new-project",        PCliNew)
            .Replace("dostar add-module",         PCliAdd)
            .Replace("dostar remove-module",      PCliRemove)
            // Apply project-name substitutions
            .Replace("piers-sinclair/Dostar", $"{githubOrg}/Dostar")
            .Replace("\"piers-sinclair\"",    $"\"{githubOrg}\"")
            .Replace("Dostar",                projectName,      StringComparison.Ordinal)
            .Replace("dostar",                projectNameLower, StringComparison.Ordinal)
            // Restore protected CLI references
            .Replace(PCliRepo,   "piers-sinclair/Dostar.Cli")
            .Replace(PCliTool,   "Dostar.Cli")
            .Replace(PCliInline, "`dostar`")
            .Replace(PCliNew,    "dostar new-project")
            .Replace(PCliAdd,    "dostar add-module")
            .Replace(PCliRemove, "dostar remove-module");
}
