using System.Text;
using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.LanguageAdapters.Portugol;

/// <summary>
/// Shared linear scanner for the Portugol family of dialects (VisuAlg, Portugol Studio).
/// Performs reverse keyword substitution while skipping strings and comments according to
/// the dialect-specific <see cref="LanguageScanRules"/>. Keeps the per-dialect adapter code
/// minimal and avoids duplicating scanner state machines.
/// </summary>
public static class PortugolScanner
{
    /// <summary>
    /// Walks the translated source code and replaces translated keywords with their original
    /// canonical text from the given keyword map. Skips string literals and comments so that
    /// keywords appearing inside them are preserved verbatim.
    /// </summary>
    /// <param name="translatedCode">Source code expressed in the natural-language keywords.</param>
    /// <param name="rules">Dialect scan rules (comment markers, single-quote support).</param>
    /// <param name="lookupTranslatedKeyword">Maps a translated word to its original keyword ID, or -1 if not a keyword.</param>
    /// <param name="idToOriginalText">Maps a keyword ID back to its canonical original text.</param>
    /// <returns>The source code with keywords reverted to their canonical form.</returns>
    public static string ReverseSubstitute(
        string translatedCode,
        LanguageScanRules rules,
        Func<string, int> lookupTranslatedKeyword,
        Func<int, string> idToOriginalText)
    {
        StringBuilder result = new StringBuilder(translatedCode.Length);
        string lineComment = rules.LineComment;
        string blockStart = rules.BlockCommentStart;
        string blockEnd = rules.BlockCommentEnd;
        int i = 0;

        while (i < translatedCode.Length)
        {
            // Skip line comments
            if (lineComment.Length > 0 && i + lineComment.Length <= translatedCode.Length
                && translatedCode.AsSpan(i, lineComment.Length).SequenceEqual(lineComment))
            {
                int lineEnd = translatedCode.IndexOf('\n', i);
                if (lineEnd < 0)
                {
                    lineEnd = translatedCode.Length;
                }
                result.Append(translatedCode, i, lineEnd - i);
                i = lineEnd;
                continue;
            }

            // Skip block comments
            if (blockStart.Length > 0 && i + blockStart.Length <= translatedCode.Length
                && translatedCode.AsSpan(i, blockStart.Length).SequenceEqual(blockStart))
            {
                int blockClose = translatedCode.IndexOf(blockEnd, i + blockStart.Length, StringComparison.Ordinal);
                int spanEnd;
                if (blockClose < 0)
                {
                    spanEnd = translatedCode.Length;
                }
                else
                {
                    spanEnd = blockClose + blockEnd.Length;
                }
                result.Append(translatedCode, i, spanEnd - i);
                i = spanEnd;
                continue;
            }

            // Skip double-quoted strings
            if (translatedCode[i] == '"')
            {
                int strStart = i;
                result.Append(translatedCode[i]);
                i++;
                while (i < translatedCode.Length && translatedCode[i] != '"')
                {
                    if (translatedCode[i] == '\\' && i + 1 < translatedCode.Length)
                    {
                        result.Append(translatedCode[i]);
                        i++;
                    }
                    if (i < translatedCode.Length)
                    {
                        result.Append(translatedCode[i]);
                        i++;
                    }
                }
                if (i < translatedCode.Length)
                {
                    result.Append(translatedCode[i]);
                    i++;
                }
                continue;
            }

            // Skip single-quoted char/string when dialect supports it
            if (rules.HasSingleQuoteStrings && translatedCode[i] == '\'')
            {
                result.Append(translatedCode[i]);
                i++;
                while (i < translatedCode.Length && translatedCode[i] != '\'')
                {
                    if (translatedCode[i] == '\\' && i + 1 < translatedCode.Length)
                    {
                        result.Append(translatedCode[i]);
                        i++;
                    }
                    if (i < translatedCode.Length)
                    {
                        result.Append(translatedCode[i]);
                        i++;
                    }
                }
                if (i < translatedCode.Length)
                {
                    result.Append(translatedCode[i]);
                    i++;
                }
                continue;
            }

            // Word tokens: try to revert translated keyword to original
            if (char.IsLetter(translatedCode[i]) || translatedCode[i] == '_')
            {
                int wordStart = i;
                while (i < translatedCode.Length && (char.IsLetterOrDigit(translatedCode[i]) || translatedCode[i] == '_'))
                {
                    i++;
                }

                string word = translatedCode.Substring(wordStart, i - wordStart);
                int keywordId = lookupTranslatedKeyword(word);

                if (keywordId >= 0)
                {
                    string originalKeyword = idToOriginalText(keywordId);
                    if (!string.IsNullOrEmpty(originalKeyword))
                    {
                        result.Append(originalKeyword);
                        continue;
                    }
                }

                result.Append(word);
                continue;
            }

            result.Append(translatedCode[i]);
            i++;
        }

        return result.ToString();
    }
}
