using System.Text.Json;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Provides default JSON serializer options used throughout the translation system.
/// </summary>
public static class JsonOptions
{
    /// <summary>
    /// Default options with case-insensitive property names and comment skipping enabled.
    /// </summary>
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
