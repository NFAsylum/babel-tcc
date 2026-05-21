namespace MultiLingualCode.Core.Tests.LanguageAdapters.Portugol;

/// <summary>
/// Shared test helpers for Portugol adapter tests (VisuAlg + Portugol Studio).
/// </summary>
public static class PortugolTestHelpers
{
    /// <summary>
    /// Builds a lookup function over a reverse-translation map that returns -1 for unknown words.
    /// Avoids ternary expressions inline in test setup.
    /// </summary>
    public static Func<string, int> MakeLookup(Dictionary<string, int> reverseMap)
    {
        return word =>
        {
            if (reverseMap.TryGetValue(word, out int id))
            {
                return id;
            }
            return -1;
        };
    }
}
