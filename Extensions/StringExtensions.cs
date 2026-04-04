namespace Dostar.Cli;

internal static class StringExtensions
{
    private static readonly Regex PascalCaseRegex = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    internal static bool IsPascalCase(this string value) => PascalCaseRegex.IsMatch(value);
}
