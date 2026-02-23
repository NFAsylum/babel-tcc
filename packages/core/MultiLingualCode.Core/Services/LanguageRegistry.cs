using System.Collections.Concurrent;
using MultiLingualCode.Core.Interfaces;
using MultiLingualCode.Core.Models;

namespace MultiLingualCode.Core.Services;

public class LanguageRegistry
{
    public ConcurrentDictionary<string, ILanguageAdapter> AdaptersByExtension = new(StringComparer.OrdinalIgnoreCase);

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

    public OperationResultGeneric<ILanguageAdapter> GetAdapter(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
        {
            return OperationResultGeneric<ILanguageAdapter>.Fail("File extension is empty.");
        }

        string normalized = NormalizeExtension(fileExtension);
        if (AdaptersByExtension.TryGetValue(normalized, out ILanguageAdapter? adapter) && adapter is not null)
        {
            return OperationResultGeneric<ILanguageAdapter>.Ok(adapter);
        }

        return OperationResultGeneric<ILanguageAdapter>.Fail($"No adapter registered for extension: {fileExtension}");
    }

    public string[] GetSupportedExtensions()
    {
        return AdaptersByExtension.Keys.Order().ToArray();
    }

    public bool IsSupported(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
        {
            return false;
        }

        return AdaptersByExtension.ContainsKey(NormalizeExtension(fileExtension));
    }

    public static string NormalizeExtension(string extension)
    {
        if (extension.StartsWith('.'))
        {
            return extension.ToLowerInvariant();
        }

        return $".{extension.ToLowerInvariant()}";
    }
}
