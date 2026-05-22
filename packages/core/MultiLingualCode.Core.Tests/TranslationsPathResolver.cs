namespace MultiLingualCode.Core.Tests;

/// <summary>
/// Resolves the absolute path to the babel-tcc-translations repository for tests.
///
/// Translations are core to the product. Tests must run against the real translation
/// tables, not a partial copy — a stale/subset TestData was the root cause of the
/// v0.9.0-beta.1 marketplace bug (only pt-br shipped). This resolver enforces that
/// tests fail loudly if the real repo is not available, rather than silently
/// degrading to a reduced set.
///
/// Resolution priority (first match wins):
///   1. Env var BABEL_TCC_TRANSLATIONS_PATH (explicit override; used by CI)
///   2. Sibling directory ../../babel-tcc-translations (typical for local dev)
///
/// No fallback — if neither resolves to a valid translations repo, throws with a
/// descriptive error and remediation steps.
/// </summary>
public static class TranslationsPathResolver
{
    public static string Resolve()
    {
        List<string> candidates = new List<string>();

        string fromEnv = Environment.GetEnvironmentVariable("BABEL_TCC_TRANSLATIONS_PATH") ?? "";
        if (!string.IsNullOrEmpty(fromEnv))
        {
            candidates.Add(fromEnv);
        }

        // Walk up from the test binary location to find the babel-tcc repo root
        // (identified by .git/.gitignore presence), then look for the translations
        // repo as a sibling. Avoids hard-coding the number of "../" components,
        // which differs between Debug/Release/published builds.
        string repoRoot = FindRepoRoot(AppContext.BaseDirectory);
        if (!string.IsNullOrEmpty(repoRoot))
        {
            string sibling = Path.GetFullPath(Path.Combine(repoRoot, "..", "babel-tcc-translations"));
            candidates.Add(sibling);
        }

        foreach (string candidate in candidates)
        {
            if (IsValidTranslationsRepo(candidate))
            {
                return candidate;
            }
        }

        string message =
            "babel-tcc-translations repo not found.\n" +
            "Tests require the real translation tables to run; no fallback is allowed.\n" +
            "Searched:\n" +
            string.Join("\n", candidates.Select(c => $"  {c}")) +
            "\n\n" +
            "Fix one of:\n" +
            "  - Clone NFAsylum/babel-tcc-translations as a sibling of this repo\n" +
            "  - Set BABEL_TCC_TRANSLATIONS_PATH env var to its absolute path";
        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Walks parent directories from start until it finds a directory containing .git
    /// (file or directory). Returns the absolute path of that directory, or empty
    /// string if not found before reaching the filesystem root.
    /// </summary>
    public static string FindRepoRoot(string start)
    {
        DirectoryInfo? current = new DirectoryInfo(start);
        while (current != null)
        {
            string gitPath = Path.Combine(current.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
            {
                return current.FullName;
            }
            current = current.Parent;
        }
        return "";
    }

    public static bool IsValidTranslationsRepo(string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return false;
        }

        string naturalLanguagesDir = Path.Combine(path, "natural-languages");
        if (!Directory.Exists(naturalLanguagesDir))
        {
            return false;
        }

        return Directory.EnumerateDirectories(naturalLanguagesDir).Any();
    }
}
