using System.Collections.Concurrent;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Registry of programming language adapters, indexed by file extension, for resolving the correct adapter for a given file type.
/// </summary>
public class LanguageRegistry
{
    /// <summary>
    /// Thread-safe dictionary mapping normalized file extensions to their language adapters (case-insensitive).
    /// </summary>
    public ConcurrentDictionary<string, ILanguageAdapter> AdaptersByExtension = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a language adapter for all file extensions it supports.
    /// </summary>
    /// <param name="adapter">The language adapter to register.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public OperationResult RegisterAdapter(ILanguageAdapter adapter)
    {
        if (adapter is null)
        {
            return OperationResult.Fail("Adapter cannot be null.");
        }

        string[] extensions = adapter.FileExtensions;
        if (extensions is null || extensions.Length == 0)
        {
            return OperationResult.Fail("Adapter must support at least one file extension.");
        }

        foreach (string ext in extensions)
        {
            string normalized = NormalizeExtension(ext);
            AdaptersByExtension[normalized] = adapter;
        }

        return OperationResult.Ok();
    }

    /// <summary>
    /// Retrieves the registered language adapter for a given file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to look up (with or without leading dot).</param>
    /// <returns>An operation result containing the adapter, or a failure if no adapter is registered for that extension.</returns>
    public OperationResultGeneric<ILanguageAdapter> GetAdapter(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
        {
            return OperationResultGeneric<ILanguageAdapter>.Fail("File extension is empty.");
        }

        string normalized = NormalizeExtension(fileExtension);
        if (AdaptersByExtension.ContainsKey(normalized))
        {
            return OperationResultGeneric<ILanguageAdapter>.Ok(AdaptersByExtension[normalized]);
        }

        return OperationResultGeneric<ILanguageAdapter>.Fail($"No adapter registered for extension: {fileExtension}");
    }

    /// <summary>
    /// Returns all registered file extensions in sorted order.
    /// </summary>
    /// <returns>An array of supported file extensions.</returns>
    public string[] GetSupportedExtensions()
    {
        return AdaptersByExtension.Keys.Order().ToArray();
    }

    /// <summary>
    /// Checks whether a language adapter is registered for the given file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to check (with or without leading dot).</param>
    /// <returns>True if an adapter is registered for the extension; false otherwise.</returns>
    public bool IsSupported(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
        {
            return false;
        }

        return AdaptersByExtension.ContainsKey(NormalizeExtension(fileExtension));
    }

    /// <summary>
    /// Normalizes a file extension to lowercase with a leading dot.
    /// </summary>
    /// <param name="extension">The file extension to normalize.</param>
    /// <returns>The normalized extension (e.g., ".cs").</returns>
    public static string NormalizeExtension(string extension)
    {
        if (extension.StartsWith('.'))
        {
            return extension.ToLowerInvariant();
        }

        return $".{extension.ToLowerInvariant()}";
    }
}
