using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy;

public class HouseEconomyItem
{
    [BsonElement("item_name")]
    public required string ItemName { get; set; }

    [BsonElement("quantity")]
    public int Quantity { get; set; } = 1;

    [BsonElement("value")]
    public long Value { get; set; } = 0;

    [BsonElement("is_stackable")]
    public bool IsStackable { get; set; } = false;

    [BsonElement("rarity")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Rarity Rarity { get; set; } = Rarity.Common;

    [BsonElement("description")]
    public string Description { get; set; } = "None provided";

    [BsonElement("is_purchasable")]
    public bool IsPurchaseable { get; set; } = true;
}

