using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.General;
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

    [BsonElement("inventory")]
    public List<HouseEconomyItem> Inventory { get; set; } = [];

    [BsonElement("amount_killed")]
    public long AmountKilled { get; set; } = 0;

    [BsonElement("killed")]
    public long Killed { get; set; } = 0;
}