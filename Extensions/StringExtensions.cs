namespace Dostar.Cli;

internal static class StringExtensions
{
    private static readonly Regex PascalCaseRegex = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    internal static bool IsPascalCase(this string value) => PascalCaseRegex.IsMatch(value);

    internal static string ToKebabCase(this string value) =>
        string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c)
            ? $"-{char.ToLower(c, CultureInfo.InvariantCulture)}"
            : char.ToLower(c, CultureInfo.InvariantCulture).ToString()));
}
