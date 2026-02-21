using System.Text.Json;

namespace MultiLingualCode.Core.Models.Translation;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
