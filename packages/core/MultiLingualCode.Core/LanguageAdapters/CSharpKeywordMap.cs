namespace MultiLingualCode.Core.LanguageAdapters;

/// <summary>
/// Provides a bidirectional mapping between C# keyword text and integer IDs.
/// </summary>
public static class CSharpKeywordMap
{
    /// <summary>
    /// Maps C# keyword text (case-insensitive) to its corresponding integer ID.
    /// </summary>
    public static readonly Dictionary<string, int> TextToId = new(StringComparer.OrdinalIgnoreCase)
    {
        ["abstract"] = 0,
        ["as"] = 1,
        ["base"] = 2,
        ["bool"] = 3,
        ["break"] = 4,
        ["byte"] = 5,
        ["case"] = 6,
        ["catch"] = 7,
        ["char"] = 8,
        ["checked"] = 9,
        ["class"] = 10,
        ["const"] = 11,
        ["continue"] = 12,
        ["decimal"] = 13,
        ["default"] = 14,
        ["delegate"] = 15,
        ["do"] = 16,
        ["double"] = 17,
        ["else"] = 18,
        ["enum"] = 19,
        ["event"] = 20,
        ["explicit"] = 21,
        ["extern"] = 22,
        ["false"] = 23,
        ["finally"] = 24,
        ["fixed"] = 25,
        ["float"] = 26,
        ["for"] = 27,
        ["foreach"] = 28,
        ["goto"] = 29,
        ["if"] = 30,
        ["implicit"] = 31,
        ["in"] = 32,
        ["int"] = 33,
        ["interface"] = 34,
        ["internal"] = 35,
        ["is"] = 36,
        ["lock"] = 37,
        ["long"] = 38,
        ["namespace"] = 39,
        ["new"] = 40,
        ["null"] = 41,
        ["object"] = 42,
        ["operator"] = 43,
        ["out"] = 44,
        ["override"] = 45,
        ["params"] = 46,
        ["private"] = 47,
        ["protected"] = 48,
        ["public"] = 49,
        ["readonly"] = 50,
        ["ref"] = 51,
        ["return"] = 52,
        ["sbyte"] = 53,
        ["sealed"] = 54,
        ["short"] = 55,
        ["sizeof"] = 56,
        ["stackalloc"] = 57,
        ["static"] = 58,
        ["string"] = 59,
        ["struct"] = 60,
        ["switch"] = 61,
        ["this"] = 62,
        ["throw"] = 63,
        ["true"] = 64,
        ["try"] = 65,
        ["typeof"] = 66,
        ["uint"] = 67,
        ["ulong"] = 68,
        ["unchecked"] = 69,
        ["unsafe"] = 70,
        ["ushort"] = 71,
        ["using"] = 72,
        ["virtual"] = 73,
        ["var"] = 74,
        ["void"] = 75,
        ["volatile"] = 76,
        ["while"] = 77,
        ["async"] = 78,
        ["await"] = 79,
        ["yield"] = 80,
        ["record"] = 81,
        ["partial"] = 82,
        ["where"] = 83,
        ["dynamic"] = 84,
        ["nameof"] = 85,
        ["init"] = 86,
        ["required"] = 87,
        ["global"] = 88,
    };

    /// <summary>
    /// Maps integer IDs back to their corresponding C# keyword text.
    /// </summary>
    public static readonly Dictionary<int, string> IdToText;

    static CSharpKeywordMap()
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
    /// <returns>A new dictionary containing all keyword text to ID mappings.</returns>
    public static Dictionary<string, int> GetMap() => new(TextToId);

    /// <summary>
    /// Gets the integer ID for a given keyword text.
    /// </summary>
    /// <param name="keywordText">The keyword text to look up.</param>
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
        if (IdToText.ContainsKey(id))
        {
            return IdToText[id];
        }

        return "";
    }

    /// <summary>
    /// Determines whether a Roslyn <see cref="Microsoft.CodeAnalysis.CSharp.SyntaxKind"/> represents a C# keyword.
    /// </summary>
    /// <param name="kind">The syntax kind to check.</param>
    /// <returns>True if the syntax kind is a keyword; otherwise false.</returns>
    public static bool IsKeyword(Microsoft.CodeAnalysis.CSharp.SyntaxKind kind)
    {
        return RoslynWrapper.IsKeywordKind(kind);
    }
}
