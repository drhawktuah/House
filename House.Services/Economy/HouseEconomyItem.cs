using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy;

public abstract class HouseEconomyItem
{
    [BsonElement("item_name")]
    public string ItemName { get; init; }

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

    protected HouseEconomyItem(string itemName)
    {
        ItemName = itemName;
    }

    public virtual HouseEconomyItem CloneWithQuantity(int quantity)
    {
        var clone = (HouseEconomyItem)MemberwiseClone();
        clone.Quantity = IsStackable ? quantity : 1;

        return clone;
    }
}