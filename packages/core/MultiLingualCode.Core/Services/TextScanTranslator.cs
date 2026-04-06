using System.Text;
using MultiLingualCode.Core.Interfaces;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Fast keyword translator using linear text scan. Skips strings, comments,
/// preprocessor directives and raw string literals. O(n) complexity.
/// Known limitation: verbatim strings @"..." with "" escape may cause
/// the scanner to exit the string early at the first "". In practice
/// this is rare and only affects keywords inside verbatim strings.
/// </summary>
public class TextScanTranslator
{
    /// <summary>
    /// Translates keywords in source code using a linear scan.
    /// </summary>
    /// <param name="code">The source code to translate.</param>
    /// <param name="translations">Map of original keyword to translated keyword.</param>
    /// <returns>The translated source code.</returns>
    public static string Translate(string code, Dictionary<string, string> translations)
    {
        StringBuilder result = new StringBuilder(code.Length);
        int i = 0;

        while (i < code.Length)
        {
            // Skip preprocessor directives (# at start of line, possibly indented)
            if (code[i] == '#' && IsStartOfLine(code, i))
            {
                int end = code.IndexOf('\n', i);
                if (end < 0) end = code.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip line comments (//)
            if (i + 1 < code.Length && code[i] == '/' && code[i + 1] == '/')
            {
                int end = code.IndexOf('\n', i);
                if (end < 0) end = code.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip block comments (/* */)
            if (i + 1 < code.Length && code[i] == '/' && code[i + 1] == '*')
            {
                int end = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (end < 0) end = code.Length - 2;
                end += 2;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip Python comments (#) — only if # is not at start of line (handled above)
            // Python files don't have preprocessor, so # mid-line is a comment
            // This is handled by the caller choosing the right comment style

            // Skip raw string literals (C# 11: """ ... """)
            if (i + 2 < code.Length && code[i] == '"' && code[i + 1] == '"' && code[i + 2] == '"')
            {
                result.Append(code[i]);
                result.Append(code[i + 1]);
                result.Append(code[i + 2]);
                i += 3;
                while (i + 2 < code.Length)
                {
                    if (code[i] == '"' && code[i + 1] == '"' && code[i + 2] == '"')
                    {
                        result.Append(code[i]);
                        result.Append(code[i + 1]);
                        result.Append(code[i + 2]);
                        i += 3;
                        break;
                    }
                    result.Append(code[i]);
                    i++;
                }
                continue;
            }

            // Skip double-quoted strings
            if (code[i] == '"')
            {
                result.Append(code[i]);
                i++;
                while (i < code.Length && code[i] != '"')
                {
                    if (code[i] == '\\') { result.Append(code[i]); i++; }
                    if (i < code.Length) { result.Append(code[i]); i++; }
                }
                if (i < code.Length) { result.Append(code[i]); i++; }
                continue;
            }

            // Skip single-quoted chars
            if (code[i] == '\'')
            {
                result.Append(code[i]);
                i++;
                while (i < code.Length && code[i] != '\'')
                {
                    if (code[i] == '\\') { result.Append(code[i]); i++; }
                    if (i < code.Length) { result.Append(code[i]); i++; }
                }
                if (i < code.Length) { result.Append(code[i]); i++; }
                continue;
            }

            // Try to match a keyword
            if (char.IsLetter(code[i]) || code[i] == '_')
            {
                int start = i;
                while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_'))
                {
                    i++;
                }
                string word = code.Substring(start, i - start);

                if (translations.TryGetValue(word, out string translated))
                {
                    result.Append(translated);
                }
                else
                {
                    result.Append(word);
                }
                continue;
            }

            result.Append(code[i]);
            i++;
        }

        return result.ToString();
    }

    /// <summary>
    /// Checks if position is at the start of a line (only whitespace before it).
    /// </summary>
    public static bool IsStartOfLine(string code, int pos)
    {
        if (pos == 0) return true;
        int j = pos - 1;
        while (j >= 0 && code[j] != '\n')
        {
            if (code[j] != ' ' && code[j] != '\t') return false;
            j--;
        }
        return true;
    }

    /// <summary>
    /// Builds a translation map (keyword → translated keyword) from a keyword map and provider.
    /// </summary>
    public static Dictionary<string, string> BuildTranslationMap(
        Dictionary<string, int> keywordMap, INaturalLanguageProvider provider)
    {
        Dictionary<string, string> map = new();
        if (keywordMap == null)
        {
            return map;
        }
        foreach (KeyValuePair<string, int> kv in keywordMap)
        {
            Models.OperationResultGeneric<string> translated = provider.TranslateKeyword(kv.Value);
            if (translated.IsSuccess)
            {
                map[kv.Key] = translated.Value;
            }
        }
        return map;
    }
}
