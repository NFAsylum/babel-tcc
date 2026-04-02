namespace MultiLingualCode.Core.LanguageAdapters.Python;

/// <summary>
/// Provides a bidirectional mapping between Python keyword text and integer IDs.
/// Python is case-sensitive: True, False, None are keywords but true, false, none are not.
/// </summary>
public static class PythonKeywordMap
{
    /// <summary>
    /// Maps Python keyword text (case-sensitive) to its corresponding integer ID.
    /// Contains all 35 hard keywords from Python 3.
    /// </summary>
    public static readonly Dictionary<string, int> TextToId = new(StringComparer.Ordinal)
    {
        ["False"] = 0,
        ["None"] = 1,
        ["True"] = 2,
        ["and"] = 3,
        ["as"] = 4,
        ["assert"] = 5,
        ["async"] = 6,
        ["await"] = 7,
        ["break"] = 8,
        ["class"] = 9,
        ["continue"] = 10,
        ["def"] = 11,
        ["del"] = 12,
        ["elif"] = 13,
        ["else"] = 14,
        ["except"] = 15,
        ["finally"] = 16,
        ["for"] = 17,
        ["from"] = 18,
        ["global"] = 19,
        ["if"] = 20,
        ["import"] = 21,
        ["in"] = 22,
        ["is"] = 23,
        ["lambda"] = 24,
        ["nonlocal"] = 25,
        ["not"] = 26,
        ["or"] = 27,
        ["pass"] = 28,
        ["raise"] = 29,
        ["return"] = 30,
        ["try"] = 31,
        ["while"] = 32,
        ["with"] = 33,
        ["yield"] = 34,
    };

    /// <summary>
    /// Maps integer IDs back to their corresponding Python keyword text.
    /// </summary>
    public static readonly Dictionary<int, string> IdToText;

    static PythonKeywordMap()
    {
        IdToText = new Dictionary<int, string>(TextToId.Count);
        foreach (KeyValuePair<string, int> kvp in TextToId)
        {
            IdToText[kvp.Value] = kvp.Key;
        }
    }

    /// <summary>
    /// Returns a copy of the keyword-to-ID dictionary.
    /// </summary>
    public static Dictionary<string, int> GetMap() => new(TextToId);

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
    /// Gets the keyword text for a given integer ID.
    /// </summary>
    /// <param name="id">The keyword ID to look up.</param>
    /// <returns>The keyword text, or an empty string if the ID is not recognized.</returns>
    public static string GetText(int id)
    {
        if (IdToText.TryGetValue(id, out string? text))
        {
            return text;
        }

        return "";
    }
}
