using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public sealed class SnipedMessage : DatabaseEntity
{
    [BsonElement("message_id")]
    public ulong MessageID { get; set; }

    [BsonElement("channel_id")]
    public ulong ChannelID { get; set; }

    [BsonElement("author_id")]
    public ulong AuthorID { get; set; }

    [BsonElement("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("deleted_at")]
    public DateTime DeletedAt { get; set; }

    public SnipedMessage()
    {
    }

    public SnipedMessage(ulong messageId, ulong channelId, ulong authorId, string authorName, string content)
    {
        MessageID = messageId;
        ChannelID = channelId;
        AuthorID = authorId;
        AuthorName = authorName;
        Content = content;
        DeletedAt = DateTime.UtcNow;
    }
}