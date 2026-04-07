using MultiLingualCode.Core.Services;

namespace MultiLingualCode.Core.Interfaces;

/// <summary>
/// Optional interface for language adapters that support fast Text Scan translation.
/// Adapters implementing this interface provide scan rules so the TextScanTranslator
/// can translate keywords without needing a full AST parse.
/// </summary>
public interface ITextScannable
{
    /// <summary>
    /// Returns the scan rules for this language (comment syntax, string delimiters, etc.).
    /// </summary>
    LanguageScanRules GetScanRules();
}
