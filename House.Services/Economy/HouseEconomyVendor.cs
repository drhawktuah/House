using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.Items;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy;

public enum VendorType
{
    General,
    WeaponDealer,
    FoodVendor,
    BlackMarketDealer,
    MedicalSupplier,
    DrugDealer,
    TechDealer
}

public class HouseEconomyVendor
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string VendorId { get; set; } = Guid.NewGuid().ToString();

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
        [Rarity.Legendary] = 3.0
    };

    [BsonElement("description")]
    public string Description { get; set; } = "A general vendor.";

    [BsonElement("quirk")]
    public string Quirk { get; set; } = "";

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

        double price = item.Value * MarkupRate * rarityMultiplier;
        return (long)Math.Ceiling(price);
    }

    public void AddItem(HouseEconomyItem item)
    {
        Inventory.Add(item);
    }

    public bool RemoveItem(string itemName)
    {
        var item = Inventory.FirstOrDefault(i => i.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            return false;
        }

        return Inventory.Remove(item);
    }
}

public static class VendorPresets
{
    public static HouseEconomyVendor GunVendor => new()
    {
        Name = "Eric Foreman",
        Type = VendorType.WeaponDealer,
        MarkupRate = 1.2,
        Description = "Disciplined, serious, and a bit of a control freak. Foreman ensures everything is in order—just don't question his pricing.",
        Inventory =
        {
            new Handgun(),
            new Shotgun(),
            new SniperRifle(),
            new Crossbow(),
            new RayGun()
        },
        Quirk = "Foreman inspects every purchase carefully. Occasionally gives a 5% discount for polite customers."
    };

    public static HouseEconomyVendor SpecialtyGunVendor => new()
    {
        Name = "Lawrence Kutner",
        Type = VendorType.BlackMarketDealer,
        MarkupRate = 2.0,
        Description = "Excitable, unpredictable, and loves extreme firepower. Buying from him is always a gamble.",
        Inventory =
        {
            new RayGun(),
            new RayGunMarkII(),
            new WunderwaffeDG2(),
            new ThunderGun(),
        },
        Quirk = "Sometimes randomly discounts a rare weapon—or sells you something you didn't ask for."
    };

    public static HouseEconomyVendor MedicalVendor => new()
    {
        Name = "Gregory House",
        Type = VendorType.MedicalSupplier,
        MarkupRate = 1.5,
        Description = "Sarcastic, brilliant, and unpredictable. Only the desperate buy from House—but the effects are always... effective.",
        Inventory =
        {
            new Medkit(),
            new Painkillers(),
            new Morphine(),
            new ExperimentalSerum(),
            new Fentanyl(),
            new Vicodin()
        },
        Quirk = "House might insult your intelligence, but grants a 10% bonus effect to any medical item purchased."
    };

    public static HouseEconomyVendor DrugVendor => new()
    {
        Name = "Robert Chase",
        Type = VendorType.DrugDealer,
        MarkupRate = 1.3,
        Description = "Charming and manipulative. Chase knows what you want before you even ask.",
        Inventory =
        {
            new CaffeinePill(),
            new AdrenalineShot(),
            new Cocaine(),
            new LSD(),
            new BlueMethamphetamine()
        },
        Quirk = "Chase flirts; sometimes gives +1 free quantity of stimulant items."
    };

    public static HouseEconomyVendor FoodVendor => new()
    {
        Name = "Allison Cameron",
        Type = VendorType.FoodVendor,
        MarkupRate = 1.1,
        Description = "Caring and considerate—she always makes sure you eat something healthy... sometimes.",
        Inventory =
        {
            new Apple(),
            new Bread(),
            new Salad(),
            new Burger(),
            new Sushi(),
            new LegendaryFeast()
        },
        Quirk = "Cameron reminds you to eat well—each purchase restores +5 temporary morale."
    };

    public static HouseEconomyVendor KnifeVendor => new()
    {
        Name = "James Wilson",
        Type = VendorType.WeaponDealer,
        MarkupRate = 1.0,
        Description = "Soft-spoken and diplomatic. He prefers knives for precise work rather than chaos.",
        Inventory =
        {
            new Katana(),
            new Dagger(),
            new BowieKnife(),
            new Karambit(),
            new Switchblade()
        },
        Quirk = "Wilson calmly explains the best usage of knives; every purchase increases critical strike chance by 2%."
    };

    public static HouseEconomyVendor TechVendor => new()
    {
        Name = "Chris Taub",
        Type = VendorType.TechDealer,
        MarkupRate = 1.2,
        Description = "Resourceful and practical, Taub always has tools and gadgets to get you out of trouble.",
        Inventory =
        {
            new Lockpick(),
            new Toolkit(),
            new Backpack()
        },
        Quirk = "Taub offers a 5% discount if you explain how the tool will be used."
    };

    public static HouseEconomyVendor ExoticDrugVendor => new()
    {
        Name = "Remy 'Thirteen' Hadley",
        Type = VendorType.DrugDealer,
        MarkupRate = 1.4,
        Description = "Mysterious and edgy. You never know exactly what she'll have, but it's usually potent.",
        Inventory =
        {
            new Methamphetamine(),
            new BlueMethamphetamine(),
            new Heroin(),
            new Fentanyl()
        },
        Quirk = "Thirteen keeps a poker face; 10% chance for items to have double duration."
    };

    public static HouseEconomyVendor SpecialtyFoodVendor => new()
    {
        Name = "Lisa Cuddy",
        Type = VendorType.FoodVendor,
        MarkupRate = 1.3,
        Description = "Organized and classy. Big-backed, too. Offers premium and healthy food options with a touch of luxury.",
        Inventory =
        {
            new LegendaryFeast(),
            new Steak(),
            new Sushi(),
            new Salad()
        },
        Quirk = "Cuddy's organized service ensures food restores 5% more health than usual."
    };
}

public static class VendorHelper
{
    public static void AddInventory(HouseEconomyVendor vendor, params (Func<int, HouseEconomyItem> ItemFactory, int Quantity)[] items)
    {
        foreach (var (factory, quantity) in items)
        {
            var item = factory(quantity);
            vendor.Inventory.Add(item);
        }
    }
}