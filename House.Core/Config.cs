using Newtonsoft.Json;

namespace House.House.Core;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public sealed class Config
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("owner_ids")]
    public ulong[] OwnerIDS { get; set; }

    [JsonProperty("default_prefixes")]
    public string[] DefaultPrefixes { get; set; }

    [JsonProperty("database_connection_url")]
    public string DBConnectionURL { get; set; }

    public static Config Deserialize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        string data = File.ReadAllText(path);
        if (data.Length == 0)
        {
            throw new JsonException($"{nameof(data)} cannot be empty");
        }

        Config? config = JsonConvert.DeserializeObject<Config>(data);
        if (config == null)
        {
            throw new JsonException($"{nameof(config)} cannot be deserialized");
        }

        return config;
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.