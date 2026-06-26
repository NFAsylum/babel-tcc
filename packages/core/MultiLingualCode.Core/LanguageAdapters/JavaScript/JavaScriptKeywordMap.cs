namespace MultiLingualCode.Core.LanguageAdapters.JavaScript;

/// <summary>
/// Provides a bidirectional mapping between JavaScript keyword text and integer IDs.
/// JavaScript is case-sensitive: "class" is a keyword but "Class" is an ordinary
/// identifier, so storage and lookups both use ordinal (case-sensitive) comparison.
/// Only reserved words that are unambiguous keywords are included. Contextual keywords
/// such as "of", "as", "from", "get" and "set" are intentionally excluded because they
/// commonly appear as identifiers or property names, and the keyword-only Text Scan path
/// cannot tell the two uses apart.
/// </summary>
public static class JavaScriptKeywordMap
{
    /// <summary>
    /// Maps JavaScript keyword text (case-sensitive) to its integer ID. IDs are assigned
    /// in alphabetical order of the keyword and must stay in sync with keywords-base.json.
    /// </summary>
    public static readonly Dictionary<string, int> TextToId = new(StringComparer.Ordinal)
    {
        ["async"] = 0,
        ["await"] = 1,
        ["break"] = 2,
        ["case"] = 3,
        ["catch"] = 4,
        ["class"] = 5,
        ["const"] = 6,
        ["continue"] = 7,
        ["default"] = 8,
        ["delete"] = 9,
        ["do"] = 10,
        ["else"] = 11,
        ["export"] = 12,
        ["extends"] = 13,
        ["false"] = 14,
        ["finally"] = 15,
        ["for"] = 16,
        ["function"] = 17,
        ["if"] = 18,
        ["import"] = 19,
        ["in"] = 20,
        ["instanceof"] = 21,
        ["let"] = 22,
        ["new"] = 23,
        ["null"] = 24,
        ["return"] = 25,
        ["static"] = 26,
        ["super"] = 27,
        ["switch"] = 28,
        ["this"] = 29,
        ["throw"] = 30,
        ["true"] = 31,
        ["try"] = 32,
        ["typeof"] = 33,
        ["var"] = 34,
        ["void"] = 35,
        ["while"] = 36,
        ["yield"] = 37,
    };

    /// <summary>
    /// Maps integer IDs back to their canonical keyword text.
    /// </summary>
    public static readonly Dictionary<int, string> IdToText;

    static JavaScriptKeywordMap()
    {
        IdToText = new Dictionary<int, string>(TextToId.Count);
        foreach (KeyValuePair<string, int> kvp in TextToId)
        {
            IdToText[kvp.Value] = kvp.Key;
        }
    }

    /// <summary>
    /// Returns a copy of the keyword-to-ID dictionary preserving the ordinal comparer.
    /// </summary>
    public static Dictionary<string, int> GetMap()
    {
        return new Dictionary<string, int>(TextToId, StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the integer ID for a given keyword text.
    /// </summary>
    /// <param name="keywordText">The keyword text to look up (case-sensitive).</param>
    /// <returns>The keyword ID, or -1 if the text is not a recognized keyword.</returns>
    public static int GetId(string keywordText)
    {
        if (TextToId.TryGetValue(keywordText, out int id))
        {
            return id;
        }

        return -1;
    }

    /// <summary>
    /// Gets the canonical keyword text for a given integer ID.
    /// </summary>
    /// <param name="id">The keyword ID to look up.</param>
    /// <returns>The keyword text, or an empty string if the ID is not recognized.</returns>
    public static string GetText(int id)
    {
        if (IdToText.ContainsKey(id))
        {
            return IdToText[id];
        }

        return "";
    }
}