namespace MultiLingualCode.Core.Tests.LanguageAdapters;

/// <summary>
/// Shared test helpers for language-adapter reverse-translation tests (C#, Python, JavaScript,
/// VisuAlg, Portugol Studio). Lives in the shared LanguageAdapters test namespace so any adapter
/// test can reuse it without depending on a sibling language's namespace.
/// </summary>
public static class AdapterTestHelpers
{
    /// <summary>
    /// Sentinel a keyword lookup returns for a word that is not a keyword. Must be negative:
    /// keyword ids start at 0 and adapters treat any id &gt;= 0 as a keyword, so defaulting an
    /// unknown word to 0 would be misread as the keyword with id 0. Centralized here so no call
    /// site can pick the wrong default.
    /// </summary>
    public const int NotAKeyword = -1;

    /// <summary>
    /// Builds a lookup over a reverse-translation map that returns the keyword id for a known
    /// word and <see cref="NotAKeyword"/> for an unknown one.
    /// </summary>
    public static Func<string, int> MakeLookup(Dictionary<string, int> reverseMap)
    {
        return word => reverseMap.GetValueOrDefault(word, NotAKeyword);
    }
}
