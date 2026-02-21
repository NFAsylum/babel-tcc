using System.Text.Json;

namespace MultiLingualCode.Core.Models.Translation;

/// <summary>
/// Shared JSON serializer options for translation models.
/// </summary>
internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
