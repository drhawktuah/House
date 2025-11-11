using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public class StarboardEntry : DatabaseEntity
{
    [BsonElement("message_id")]
    public ulong MessageID { get; set; } = default!;

    [BsonElement("starboard_message_id")]
    public ulong StarboardMessageID { get; set; } = default!;

    [BsonElement("guild_id")]
    public ulong GuildID { get; set; } = default!;
}
