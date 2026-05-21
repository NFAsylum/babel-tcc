namespace MultiLingualCode.Core.LanguageAdapters.Portugol;

/// <summary>
/// Provides a bidirectional mapping between Portugol Studio keyword text and integer IDs.
/// Portugol Studio (UNIVALI dialect) is case-sensitive and uses C-like block structure.
/// Keyword set sourced from the Portugol Webstudio ANTLR4 grammar
/// (https://github.com/dgadelha/Portugol-Webstudio) covering 26 reserved words.
/// </summary>
public static class PortugolStudioKeywordMap
{
    /// <summary>
    /// Maps Portugol Studio keyword text (case-sensitive) to its integer ID.
    /// </summary>
    public static readonly Dictionary<string, int> TextToId = new(StringComparer.Ordinal)
    {
        // Data types (0-5)
        ["inteiro"] = 0,
        ["real"] = 1,
        ["caracter"] = 2,
        ["cadeia"] = 3,
        ["logico"] = 4,
        ["vazio"] = 5,

        // Control flow (6-15)
        ["se"] = 6,
        ["senao"] = 7,
        ["enquanto"] = 8,
        ["faca"] = 9,
        ["para"] = 10,
        ["escolha"] = 11,
        ["caso"] = 12,
        ["contrario"] = 13,
        ["pare"] = 14,
        ["retorne"] = 15,

        // Declarations (16-18)
        ["programa"] = 16,
        ["funcao"] = 17,
        ["const"] = 18,

        // Library (19-20)
        ["inclua"] = 19,
        ["biblioteca"] = 20,

        // Logical operators (21-23)
        ["e"] = 21,
        ["ou"] = 22,
        ["nao"] = 23,

        // Boolean literals (24-25)
        ["verdadeiro"] = 24,
        ["falso"] = 25,
    };

    /// <summary>
    /// Maps integer IDs back to their canonical keyword text.
    /// </summary>
    public static readonly Dictionary<int, string> IdToText;

    static PortugolStudioKeywordMap()
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
