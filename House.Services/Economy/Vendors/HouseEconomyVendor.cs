using System.Security.Cryptography;
using House.House.Services.Economy.General;
using House.House.Services.Economy.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Vendors;

public class HouseEconomyVendor
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string VendorId { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("aliases")]
    public required List<string> Aliases { get; set; }

    [BsonElement("vendor_name")]
    public required string Name { get; set; }

    [BsonElement("vendor_type")]
    public VendorType Type { get; set; } = VendorType.General;

    [BsonElement("inventory")]
    public List<HouseEconomyItem> Inventory { get; set; } = [];

    [BsonElement("markup_rate")]
    public double MarkupRate { get; set; } = 1.0;

    [BsonElement("rarity_price_multiplier")]
    public Dictionary<Rarity, double> RarityPriceMultiplier { get; set; } = new()
    {
        [Rarity.Common] = 1.0,
        [Rarity.Uncommon] = 1.25,
        [Rarity.Rare] = 1.5,
        [Rarity.Epic] = 2.0,
        [Rarity.Legendary] = 3.0,
        [Rarity.WonderWeapon] = 5.0
    };

    [BsonElement("description")]
    public string Description { get; set; } = "A general vendor.";

    [BsonElement("quirk")]
    public string Quirk { get; set; } = "";

    [BsonElement("restock_interval_hours")]
    public TimeSpan RestockInterval { get; set; } = TimeSpan.FromMinutes(1);

    [BsonElement("last_restock_time")]
    public DateTime LastRestockTime { get; set; } = DateTime.UtcNow;

    public long GetPrice(HouseEconomyItem item)
    {
        double rarityMultiplier;
        if (RarityPriceMultiplier.TryGetValue(item.Rarity, out var multiplier))
        {
            rarityMultiplier = (double)multiplier;
        }
        else
        {
            rarityMultiplier = (double)1.0;
        }

        return (long)Math.Ceiling(item.Value * MarkupRate * rarityMultiplier);
    }

    public int UpdateInventory()
    {
        var possibilities = GlobalItemPool.FilterForVendor(Type).ToList();
        if (possibilities.Count == 0)
        {
            return 0;
        }

        int minItems = Math.Max(5, possibilities.Count / 5);
        int maxItems = Math.Max(minItems + 2, possibilities.Count * 3 / 5);
        int newItemCount = NextInt(minItems, maxItems);

        var newItems = possibilities
            .OrderBy(_ => NextDouble())
            .Take(newItemCount)
            .Select(item =>
            {
                var clone = item.CloneWithQuantity(NextInt(1, 5));

                clone.Value = CalculateValueForitem(clone);

                return clone;
            })
            .ToList();

        Inventory.Clear();
        Inventory.AddRange(newItems);

        LastRestockTime = DateTime.UtcNow;

        return newItems.Count;
    }

    private static long CalculateValueForitem(HouseEconomyItem item)
    {
        long baseValue = item.Value;

        return item.Rarity switch
        {
            Rarity.Common => (long)(baseValue * (0.9 + NextDouble() * 0.2)),
            Rarity.Uncommon => (long)(baseValue * (1.0 + NextDouble() * 0.3)),
            Rarity.Rare => (long)(baseValue * (1.2 + NextDouble() * 0.4)),
            Rarity.Epic => (long)(baseValue * (1.5 + NextDouble() * 0.6)),
            Rarity.Legendary => (long)(baseValue * (2.0 + NextDouble() * 1.0)),
            Rarity.WonderWeapon => (long)(baseValue * (3.0 + NextDouble() * 2.0)),
            _ => baseValue
        };
    }

    /*
    private static Rarity GetRarityForVendor(VendorType type, double roll) => type switch
    {
        VendorType.General => roll switch
        {
            < 0.70 => Rarity.Common,
            < 0.90 => Rarity.Uncommon,
            < 0.97 => Rarity.Rare,
            < 0.995 => Rarity.Epic,
            < 0.999 => Rarity.Legendary,
            _ => Rarity.WonderWeapon
        },
        VendorType.BlackMarket => roll switch
        {
            < 0.30 => Rarity.Common,
            < 0.60 => Rarity.Uncommon,
            < 0.80 => Rarity.Rare,
            < 0.95 => Rarity.Epic,
            < 0.995 => Rarity.Legendary,
            _ => Rarity.WonderWeapon
        },
        VendorType.Medical => roll switch
        {
            < 0.80 => Rarity.Common,
            < 0.95 => Rarity.Uncommon,
            < 0.99 => Rarity.Rare,
            _ => Rarity.Epic
        },
        VendorType.Food => roll switch
        {
            < 0.90 => Rarity.Common,
            < 0.98 => Rarity.Uncommon,
            < 0.995 => Rarity.Rare,
            _ => Rarity.Epic
        },
        VendorType.Tool => roll switch
        {
            < 0.60 => Rarity.Common,
            < 0.85 => Rarity.Uncommon,
            < 0.97 => Rarity.Rare,
            < 0.995 => Rarity.Epic,
            _ => Rarity.Legendary
        },
        _ => Rarity.Common
    };
    */

    private static int NextInt(int min, int max) => RandomNumberGenerator.GetInt32(min, max);

    private static double NextDouble()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return (BitConverter.ToUInt64(bytes) >> 11) / (double)(1UL << 53);
    }
}
