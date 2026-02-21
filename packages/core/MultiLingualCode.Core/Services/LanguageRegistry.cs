using System.Collections.Concurrent;
using MultiLingualCode.Core.Interfaces;

namespace MultiLingualCode.Core.Services;

/// <summary>
/// Registry that maps file extensions to language adapters.
/// Used by the translation pipeline to find the correct adapter for a given file.
/// </summary>
public class LanguageRegistry
{
    private readonly ConcurrentDictionary<string, ILanguageAdapter> _adaptersByExtension = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a language adapter for all file extensions it supports.
    /// If an extension is already registered, the new adapter replaces the previous one.
    /// </summary>
    /// <param name="adapter">The language adapter to register.</param>
    /// <exception cref="ArgumentNullException">If adapter is null.</exception>
    /// <exception cref="ArgumentException">If the adapter reports no file extensions.</exception>
    public void RegisterAdapter(ILanguageAdapter adapter)
    {
        ArgumentNullException.ThrowIfNull(adapter);

        var extensions = adapter.FileExtensions;
        if (extensions is null || extensions.Length == 0)
            throw new ArgumentException("Adapter must support at least one file extension.", nameof(adapter));

        foreach (var ext in extensions)
        {
            var normalized = NormalizeExtension(ext);
            _adaptersByExtension[normalized] = adapter;
        }
    }

    /// <summary>
    /// Returns the adapter registered for the given file extension, or null if none is registered.
    /// </summary>
    /// <param name="fileExtension">File extension including the dot (e.g. ".cs").</param>
    /// <returns>The registered adapter, or null.</returns>
    public ILanguageAdapter? GetAdapter(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
            return null;

        var normalized = NormalizeExtension(fileExtension);
        return _adaptersByExtension.TryGetValue(normalized, out var adapter) ? adapter : null;
    }

    /// <summary>
    /// Returns all file extensions that have a registered adapter.
    /// </summary>
    /// <returns>Array of registered file extensions (e.g. [".cs", ".py"]).</returns>
    public string[] GetSupportedExtensions()
    {
        return _adaptersByExtension.Keys.Order().ToArray();
    }

    /// <summary>
    /// Checks whether a file extension has a registered adapter.
    /// </summary>
    /// <param name="fileExtension">File extension including the dot (e.g. ".cs").</param>
    /// <returns>True if an adapter is registered for this extension.</returns>
    public bool IsSupported(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
            return false;

        return _adaptersByExtension.ContainsKey(NormalizeExtension(fileExtension));
    }

    private static string NormalizeExtension(string extension)
    {
        return extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
    }
}
