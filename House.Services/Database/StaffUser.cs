using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public class StaffUser : DatabaseUser
{
    [BsonElement("position")]
    public Position Position { get; set; }
}
