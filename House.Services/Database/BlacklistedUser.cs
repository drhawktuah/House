using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public class BlacklistedUser : DatabaseUser
{
    [BsonElement("blacklistReason")]
    public string? Reason { get; set; }

    [BsonElement("blacklistedAt")]
    public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
}
