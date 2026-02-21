namespace MultiLingualCode.Core.Utilities;

public static class StringExtensions
{
    public static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string pascal = ToPascalCase(value);
        if (pascal.Length == 0)
        {
            return pascal;
        }

        int i = 0;
        while (i < pascal.Length && char.IsUpper(pascal[i]))
        {
            i++;
        }

        if (i == 0)
        {
            return pascal;
        }

        if (i == pascal.Length)
        {
            return pascal.ToLowerInvariant();
        }

        if (i > 1)
        {
            return pascal[..(i - 1)].ToLowerInvariant() + pascal[(i - 1)..];
        }

        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    public static string ToPascalCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        string[] words = SplitIntoWords(value);
        return string.Concat(words.Select(w =>
            w.Length == 0 ? "" : char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()
        ));
    }

    public static string[] SplitIntoWords(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return [];
        }

        List<string> words = new List<string>();
        int start = 0;

        for (int i = 1; i < value.Length; i++)
        {
            if (value[i] == '_' || value[i] == '-')
            {
                if (i > start)
                {
                    words.Add(value[start..i]);
                }
                start = i + 1;
                continue;
            }

            if (char.IsUpper(value[i]) && !char.IsUpper(value[i - 1]))
            {
                words.Add(value[start..i]);
                start = i;
                continue;
            }

            if (char.IsUpper(value[i - 1]) && char.IsUpper(value[i]) &&
                i + 1 < value.Length && char.IsLower(value[i + 1]))
            {
                words.Add(value[start..i]);
                start = i;
            }
        }

        if (start < value.Length)
        {
            words.Add(value[start..]);
        }

        return words.ToArray();
    }
}
