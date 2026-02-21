namespace MultiLingualCode.Core.Utilities;

/// <summary>
/// Extension methods for string manipulation, especially for identifier naming conventions.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to camelCase (first letter lowercase, rest follows PascalCase boundaries).
    /// Examples: "GetValue" -> "getValue", "HTML" -> "html", "myVar" -> "myVar"
    /// </summary>
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var pascal = ToPascalCase(value);
        if (pascal.Length == 0)
            return pascal;

        // Find the end of the leading uppercase sequence
        int i = 0;
        while (i < pascal.Length && char.IsUpper(pascal[i]))
            i++;

        if (i == 0)
            return pascal;

        // If entire string is uppercase, lowercase it all
        if (i == pascal.Length)
            return pascal.ToLowerInvariant();

        // If multiple leading uppercase (e.g. "HTMLParser"), lowercase all but last
        if (i > 1)
            return pascal[..(i - 1)].ToLowerInvariant() + pascal[(i - 1)..];

        // Single leading uppercase: just lowercase the first char
        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// Examples: "get_value" -> "GetValue", "getValue" -> "GetValue", "my-var" -> "MyVar"
    /// </summary>
    public static string ToPascalCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var words = SplitIntoWords(value);
        return string.Concat(words.Select(w =>
            w.Length == 0 ? "" : char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()
        ));
    }

    /// <summary>
    /// Splits an identifier into its constituent words, handling camelCase, PascalCase,
    /// snake_case, and kebab-case.
    /// Examples: "getValue" -> ["get", "Value"], "get_value" -> ["get", "value"]
    /// </summary>
    public static string[] SplitIntoWords(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return [];

        var words = new List<string>();
        int start = 0;

        for (int i = 1; i < value.Length; i++)
        {
            // Split on separators
            if (value[i] == '_' || value[i] == '-')
            {
                if (i > start)
                    words.Add(value[start..i]);
                start = i + 1;
                continue;
            }

            // Split on camelCase/PascalCase boundaries
            if (char.IsUpper(value[i]) && !char.IsUpper(value[i - 1]))
            {
                words.Add(value[start..i]);
                start = i;
                continue;
            }

            // Handle acronyms (e.g. "HTMLParser" -> split before 'P')
            if (char.IsUpper(value[i - 1]) && char.IsUpper(value[i]) &&
                i + 1 < value.Length && char.IsLower(value[i + 1]))
            {
                words.Add(value[start..i]);
                start = i;
            }
        }

        if (start < value.Length)
            words.Add(value[start..]);

        return words.ToArray();
    }
}
