using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public class WhitelistedUser : DatabaseUser
{
    [BsonElement("whitelistReason")]
    public string? Reason { get; set; }

    [BsonElement("whitelistedAt")]
    public DateTime WhitelistedAt { get; set; } = DateTime.UtcNow;
}
