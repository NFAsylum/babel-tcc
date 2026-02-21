namespace MultiLingualCode.Core.LanguageAdapters;

public static class CSharpKeywordMap
{
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
        ["void"] = 75,
        ["volatile"] = 76,
        ["while"] = 77,
    };

    public static readonly Dictionary<int, string> IdToText;

    static CSharpKeywordMap()
    {
        IdToText = new Dictionary<int, string>(TextToId.Count);
        foreach (KeyValuePair<string, int> kvp in TextToId)
        {
            IdToText[kvp.Value] = kvp.Key;
        }
    }

    public static Dictionary<string, int> GetMap() => new(TextToId);

    public static int GetId(string keywordText)
    {
        if (TextToId.TryGetValue(keywordText, out int id))
        {
            return id;
        }

        return -1;
    }

    public static string GetText(int id)
    {
        if (IdToText.TryGetValue(id, out string? text) && text is not null)
        {
            return text;
        }

        return "";
    }

    public static bool IsKeyword(Microsoft.CodeAnalysis.CSharp.SyntaxKind kind)
    {
        return RoslynWrapper.IsKeywordKind(kind);
    }
}
