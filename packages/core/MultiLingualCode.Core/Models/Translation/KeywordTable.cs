using System.Text.Json.Serialization;
using MultiLingualCode.Core.Models;
using MultiLingualCode.Core.Utilities;

namespace MultiLingualCode.Core.Models.Translation;

public class KeywordTable
{
    public Dictionary<string, int> KeywordToId = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<int, string> IdToKeyword = new();

    [JsonPropertyName("keywords")]
    public Dictionary<string, int> Keywords
    {
        get => KeywordToId;
        set
        {
            KeywordToId = new Dictionary<string, int>(value, StringComparer.OrdinalIgnoreCase);
            IdToKeyword = new Dictionary<int, string>(value.Count);
            foreach (KeyValuePair<string, int> kvp in value)
            {
                IdToKeyword[kvp.Value] = kvp.Key;
            }
        }
    }

    public int GetKeywordId(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return -1;
        }

        if (KeywordToId.TryGetValue(keyword, out int id))
        {
            return id;
        }

        return -1;
    }

    public OperationResultGeneric<string> GetKeyword(int id)
    {
        if (IdToKeyword.TryGetValue(id, out string? keyword) && keyword is not null)
        {
            return OperationResultGeneric<string>.Ok(keyword);
        }

        return OperationResultGeneric<string>.Fail($"Keyword not found for id: {id}");
    }

    public int Count => KeywordToId.Count;

    public static OperationResultGeneric<KeywordTable> LoadFrom(string filePath)
    {
        return JsonFileReader.ReadFromFile<KeywordTable>(filePath, JsonOptions.Default);
    }

    public static async Task<OperationResultGeneric<KeywordTable>> LoadFromAsync(string filePath)
    {
        return await JsonFileReader.ReadFromFileAsync<KeywordTable>(filePath, JsonOptions.Default);
    }
}
