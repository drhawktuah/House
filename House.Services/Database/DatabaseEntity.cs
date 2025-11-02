using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public abstract class DatabaseEntity
{
    [BsonId]
    public ulong ID { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DatabaseEntity()
    {

    }

    public DatabaseEntity(ulong ID)
    {
        this.ID = ID;
    }
}

/*
public abstract class DatabaseEntity<TKey>
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public TKey ID { get; set; } = default!;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
*/