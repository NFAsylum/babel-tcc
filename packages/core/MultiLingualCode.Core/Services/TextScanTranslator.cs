using System.Text;
using MultiLingualCode.Core.Interfaces;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Fast keyword translator using linear text scan. Skips strings, comments,
/// preprocessor directives and raw/triple-quoted string literals. O(n) complexity.
/// Configurable per language via LanguageScanRules.
/// Known limitation: verbatim strings @"..." with "" escape may cause
/// the scanner to exit the string early at the first "".
/// </summary>
public class TextScanTranslator
{
    /// <summary>
    /// Translates keywords in source code using a linear scan with language-specific rules.
    /// </summary>
    public static string Translate(string code, Dictionary<string, string> translations, LanguageScanRules rules)
    {
        StringBuilder result = new StringBuilder(code.Length);
        int i = 0;
        string lineComment = rules.LineComment;
        string blockStart = rules.BlockCommentStart;
        string blockEnd = rules.BlockCommentEnd;

        while (i < code.Length)
        {
            // Skip preprocessor directives (# at start of line)
            if (rules.HasPreprocessor && code[i] == '#' && IsStartOfLine(code, i))
            {
                int end = code.IndexOf('\n', i);
                if (end < 0) end = code.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip line comments
            if (lineComment.Length > 0 && i + lineComment.Length <= code.Length
                && code.AsSpan(i, lineComment.Length).SequenceEqual(lineComment))
            {
                int end = code.IndexOf('\n', i);
                if (end < 0) end = code.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip block comments
            if (blockStart.Length > 0 && i + blockStart.Length <= code.Length
                && code.AsSpan(i, blockStart.Length).SequenceEqual(blockStart))
            {
                int end = code.IndexOf(blockEnd, i + blockStart.Length, StringComparison.Ordinal);
                if (end < 0) end = code.Length - blockEnd.Length;
                end += blockEnd.Length;
                result.Append(code, i, end - i);
                i = end;
                continue;
            }

            // Skip triple-quoted strings (""" or ''')
            if (rules.HasTripleQuoteStrings && i + 2 < code.Length)
            {
                if ((code[i] == '"' && code[i + 1] == '"' && code[i + 2] == '"')
                    || (code[i] == '\'' && code[i + 1] == '\'' && code[i + 2] == '\''))
                {
                    char quoteChar = code[i];
                    result.Append(code[i]);
                    result.Append(code[i + 1]);
                    result.Append(code[i + 2]);
                    i += 3;
                    while (i + 2 < code.Length)
                    {
                        if (code[i] == quoteChar && code[i + 1] == quoteChar && code[i + 2] == quoteChar)
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

            // Skip single-quoted strings/chars
            if (rules.HasSingleQuoteStrings && code[i] == '\'')
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

                bool isEscapedIdentifier = rules.HasEscapedIdentifiers && start > 0 && code[start - 1] == '@';

                if (!isEscapedIdentifier && translations.TryGetValue(word, out string? translated))
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
    /// Overload that uses C# rules for backward compatibility.
    /// </summary>
    public static string Translate(string code, Dictionary<string, string> translations)
    {
        return Translate(code, translations, LanguageScanRules.CSharp);
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
