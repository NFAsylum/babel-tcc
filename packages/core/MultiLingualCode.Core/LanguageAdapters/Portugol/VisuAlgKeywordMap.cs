namespace MultiLingualCode.Core.LanguageAdapters.Portugol;

/// <summary>
/// Provides a bidirectional mapping between VisuAlg keyword text and integer IDs.
/// VisuAlg is case-insensitive: SE, Se, and se are all the same keyword. Canonical
/// storage is lowercase; lookups use OrdinalIgnoreCase comparison.
/// Keyword set sourced from the VisuAlgCode open-source reference implementation
/// (https://github.com/the-spanish-guy/visualgcode) covering 48 reserved words.
/// </summary>
public static class VisuAlgKeywordMap
{
    /// <summary>
    /// Maps VisuAlg keyword text (lowercase canonical, case-insensitive lookup) to its integer ID.
    /// </summary>
    public static readonly Dictionary<string, int> TextToId = new(StringComparer.OrdinalIgnoreCase)
    {
        // Program structure (0-3)
        ["algoritmo"] = 0,
        ["fimalgoritmo"] = 1,
        ["var"] = 2,
        ["inicio"] = 3,

        // Data types (4-8)
        ["inteiro"] = 4,
        ["real"] = 5,
        ["caractere"] = 6,
        ["logico"] = 7,
        ["vetor"] = 8,

        // Input/output (9-11)
        ["escreva"] = 9,
        ["escreval"] = 10,
        ["leia"] = 11,

        // Conditional (12-15)
        ["se"] = 12,
        ["entao"] = 13,
        ["senao"] = 14,
        ["fimse"] = 15,

        // Loops (16-24)
        ["para"] = 16,
        ["de"] = 17,
        ["ate"] = 18,
        ["faca"] = 19,
        ["fimpara"] = 20,
        ["enquanto"] = 21,
        ["fimenquanto"] = 22,
        ["repita"] = 23,
        ["fimrepita"] = 24,

        // Logical operators (25-27)
        ["e"] = 25,
        ["ou"] = 26,
        ["nao"] = 27,

        // Arithmetic operators (28-29)
        ["div"] = 28,
        ["mod"] = 29,

        // Booleans (30-31)
        ["verdadeiro"] = 30,
        ["falso"] = 31,

        // Subprograms (32-36)
        ["procedimento"] = 32,
        ["fimprocedimento"] = 33,
        ["funcao"] = 34,
        ["fimfuncao"] = 35,
        ["retorne"] = 36,

        // Switch (37-40)
        ["escolha"] = 37,
        ["caso"] = 38,
        ["outrocaso"] = 39,
        ["fimescolha"] = 40,

        // Other (41-47)
        ["constante"] = 41,
        ["interrompa"] = 42,
        ["xou"] = 43,
        ["limpatela"] = 44,
        ["pausa"] = 45,
        ["aleatorio"] = 46,
        ["debug"] = 47,
    };

    /// <summary>
    /// Maps integer IDs back to their canonical lowercase keyword text.
    /// </summary>
    public static readonly Dictionary<int, string> IdToText;

    static VisuAlgKeywordMap()
    {
        IdToText = new Dictionary<int, string>(TextToId.Count);
        foreach (KeyValuePair<string, int> kvp in TextToId)
        {
            IdToText[kvp.Value] = kvp.Key;
        }
    }

    /// <summary>
    /// Returns a copy of the keyword-to-ID dictionary preserving the case-insensitive comparer.
    /// </summary>
    public static Dictionary<string, int> GetMap()
    {
        return new Dictionary<string, int>(TextToId, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the integer ID for a given keyword text.
    /// </summary>
    /// <param name="keywordText">The keyword text to look up (case-insensitive).</param>
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
    /// Gets the canonical lowercase keyword text for a given integer ID.
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
