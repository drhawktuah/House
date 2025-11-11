using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy;

public class HouseEconomyUser
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public ulong ID { get; set; }

    [BsonElement("bank")]
    public long Bank { get; set; } = 0;

    [BsonElement("cash")]
    public long Cash { get; set; } = 0;
}