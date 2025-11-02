using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public class DatabaseUser : DatabaseEntity
{
    [BsonElement("username")]
    public string Username { get; set; } = null!;
}