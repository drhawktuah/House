using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Items;

public enum AmmoType
{
    None = -1,
    NineMM = 0,
    TwelveGauge = 1,
    ThreeOhEight = 2,
    Bolts = 3,
    PlasmaCell = 4,
    Arc = 5,
    Air = 6
}


public class Gun : HouseEconomyItem
{
    [BsonElement("is_special")]
    public bool IsSpecial { get; set; } = false;

    [BsonElement("damage")]
    public float Damage { get; set; } = 0f;

    [BsonElement("range")]
    public float Range { get; set; } = 0f;

    [BsonElement("description")]
    public string Description { get; set; } = "None provided";

    [BsonElement("ammo_type")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public AmmoType AmmoType { get; set; } = AmmoType.None;

    [BsonElement("magazine_size")]
    public int MagazineSize { get; set; } = 0;

    [BsonElement("fire_rate")]
    public float FireRate { get; set; } = 0f;

    [BsonElement("is_purchasable")]
    public bool IsPurchaseable { get; set; } = true;
}

public sealed class Handgun : Gun
{
    public Handgun(int quantity = 1)
    {
        ItemName = "Handgun";
        Quantity = quantity;
        Value = 1500;
        IsSingularItem = true;

        Damage = 35;
        Range = 50f;
        AmmoType = AmmoType.NineMM;
        MagazineSize = 15;
        FireRate = 3.0f;

        Description = "A basic sidearm for close encounters.";

        Rarity = Rarity.Common;
    }
}

public sealed class Shotgun : Gun
{
    public Shotgun(int quantity = 1)
    {
        ItemName = "Shotgun";
        Quantity = quantity;
        Value = 3500;
        IsSingularItem = true;

        Damage = 90;
        Range = 25f;
        AmmoType = AmmoType.TwelveGauge;
        MagazineSize = 8;
        FireRate = 1.2f;

        Description = "Close-range beast. One shot, one chunk.";

        Rarity = Rarity.Uncommon;
    }
}

public sealed class SniperRifle : Gun
{
    public SniperRifle(int quantity = 1)
    {
        ItemName = "Sniper Rifle";
        Quantity = quantity;
        Value = 7000;
        IsSingularItem = true;

        Damage = 150;
        Range = 200f;
        AmmoType = AmmoType.ThreeOhEight;
        MagazineSize = 5;
        FireRate = 0.8f;

        Description = "For when you need to delete something from across the map.";

        Rarity = Rarity.Rare;
    }
}

public sealed class Crossbow : Gun
{
    public Crossbow(int quantity = 1)
    {
        ItemName = "Crossbow";
        Quantity = quantity;
        Value = 750;
        IsSingularItem = true;

        Damage = 60;
        Range = 40f;
        AmmoType = AmmoType.Bolts;
        MagazineSize = 1;
        FireRate = 0.6f;

        Description = "Silent, deadly, and makes you feel like a medieval assassin.";
        
        Rarity = Rarity.Uncommon;
    }
}

// lol funny cod references

public sealed class RayGun : Gun
{
    public RayGun()
    {
        ItemName = "Ray Gun";
        Quantity = 1;
        Value = 100_000;
        IsSingularItem = true;
        IsSpecial = true;

        Damage = 350;
        Range = 250f;
        AmmoType = AmmoType.PlasmaCell;
        MagazineSize = 20;
        FireRate = 2.5f;

        IsPurchaseable = false;

        Description = "Classic alien blaster. Pew pew your way to round 100.";

        Rarity = Rarity.WonderWeapon;
    }
}

public sealed class RayGunMarkII : Gun
{
    public RayGunMarkII()
    {
        ItemName = "Ray Gun";
        Quantity = 1;
        Value = 150_000;
        IsSingularItem = true;
        IsSpecial = true;

        Damage = 275;
        Range = 185f;
        AmmoType = AmmoType.PlasmaCell;
        MagazineSize = 21;
        FireRate = 5.5f;

        IsPurchaseable = false;

        Description = "Burst-fire energy weapon that makes zombies evaporate.";

        Rarity = Rarity.WonderWeapon;
    }
}

public sealed class WunderwaffeDG2 : Gun
{
    public WunderwaffeDG2()
    {
        ItemName = "Wunderwaffe DG-2";
        Quantity = 1;
        Value = 10_000_000;
        IsSingularItem = true;
        IsSpecial = true;

        Damage = 10000;
        Range = 50f;
        AmmoType = AmmoType.Arc;
        MagazineSize = 3;
        FireRate = 0.5f;

        IsPurchaseable = false;

        Description = "Harness the power of electricity. Don't cross the streams.";

        Rarity = Rarity.WonderWeapon;
    }
}

public sealed class ThunderGun : Gun
{
    public ThunderGun()
    {
        ItemName = "Thunder Gun";
        Quantity = 1;
        Value = 10_000_000;
        IsSingularItem = true;
        IsSpecial = true;

        Damage = 10000;
        Range = 25f;
        AmmoType = AmmoType.Air;
        MagazineSize = 4;
        FireRate = 0.25f;

        IsPurchaseable = false;

        Description = "Sends zombies flying. Literally.";

        Rarity = Rarity.WonderWeapon;
    }
}