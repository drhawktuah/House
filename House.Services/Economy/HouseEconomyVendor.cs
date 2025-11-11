using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy;

public class HouseEconomyVendor
{
    [BsonElement("vendor_name")]
    public required string Name { get; set; }

    [BsonElement("inventory")]
    public List<HouseEconomyItem> Inventory { get; set; } = [];

    [BsonElement("currency_multiplier")]
    public double CurrencyMultiplier { get; set; } = 1.0;

    public HouseEconomyVendor(string name, double currencyMultiplier = 1.0)
    {
        Name = name;
        CurrencyMultiplier = currencyMultiplier;
    }

    public long GetPrice(HouseEconomyItem item)
    {
        return (long)(item.Value * CurrencyMultiplier);
    }

    public void AddItem(HouseEconomyItem item)
    {
        Inventory.Add(item);
    }
}