using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.General;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Items;

public class Gun : HouseEconomyItem
{
    [BsonElement("is_special")]
    public bool IsSpecial { get; set; } = false;

    [BsonElement("damage")]
    public float Damage { get; set; } = 0f;

    [BsonElement("range")]
    public float Range { get; set; } = 0f;

    [BsonElement("ammo_type")]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public AmmoType AmmoType { get; set; } = AmmoType.None;

    [BsonElement("magazine_size")]
    public int MagazineSize { get; set; } = 0;

    [BsonElement("fire_rate")]
    public float FireRate { get; set; } = 0f;

    protected Gun(string itemName) : base(itemName, HouseItemType.Firearm)
    {
        IsStackable = true;
        IsPurchaseable = true;
    }
}

public sealed class Handgun : Gun
{
    public Handgun(int quantity = 1) : base("Handgun")
    {
        Quantity = quantity;
        Value = 1500;
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
    public Shotgun(int quantity = 1) : base("Shotgun")
    {
        Quantity = quantity;
        Value = 3500;
        Damage = 90;
        Range = 25f;
        AmmoType = AmmoType.TwelveGauge;
        MagazineSize = 8;
        FireRate = 1.2f;

        Description = "Close-range powerhouse. One shot, one chunk.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class AssaultRifle : Gun
{
    public AssaultRifle(int quantity = 1) : base("Assault Rifle")
    {
        Quantity = quantity;
        Value = 5000;
        Damage = 45;
        Range = 100f;
        AmmoType = AmmoType.FiveFiveSix;
        MagazineSize = 45;
        FireRate = 8.5f;

        Description = "Versatile automatic rifle for any engagement.";
        Rarity = Rarity.Rare;
    }
}

public sealed class SniperRifle : Gun
{
    public SniperRifle(int quantity = 1) : base("Sniper Rifle")
    {
        Quantity = quantity;
        Value = 7000;
        Damage = 150;
        Range = 200f;
        AmmoType = AmmoType.ThreeOhEight;
        MagazineSize = 5;
        FireRate = 0.8f;

        Description = "Extreme precision from long distances.";
        Rarity = Rarity.Rare;
    }
}

public sealed class LMG : Gun
{
    public LMG(int quantity = 1) : base("Light Machine Gun")
    {
        Quantity = quantity;
        Value = 6000;
        Damage = 60;
        Range = 80f;
        AmmoType = AmmoType.SevenSixTwo;
        MagazineSize = 100;
        FireRate = 6.0f;

        Description = "High-capacity automatic weapon. Spray and pray.";
        Rarity = Rarity.Epic;
    }
}

public sealed class Crossbow : Gun
{
    public Crossbow(int quantity = 1) : base("Crossbow")
    {
        Quantity = quantity;
        Value = 750;
        Damage = 60;
        Range = 40f;
        AmmoType = AmmoType.Bolts;
        MagazineSize = 1;
        FireRate = 0.6f;

        Description = "Silent and deadly — the assassin’s choice.";
        Rarity = Rarity.Uncommon;
    }
}

// lol funny cod references

public sealed class RayGun : Gun
{
    public RayGun() : base("Ray Gun")
    {
        Quantity = 1;
        Value = 100_000;
        IsSpecial = true;
        IsPurchaseable = false;

        Damage = 350;
        Range = 250f;
        AmmoType = AmmoType.PlasmaCell;
        MagazineSize = 20;
        FireRate = 2.5f;

        Description = "Classic alien blaster. Pew pew your way to round 100.";
        Rarity = Rarity.WonderWeapon;
    }
}

public sealed class RayGunMarkII : Gun
{
    public RayGunMarkII() : base("Ray Gun Mark II")
    {
        Quantity = 1;
        Value = 150_000;
        IsSpecial = true;
        IsPurchaseable = false;

        Damage = 275;
        Range = 185f;
        AmmoType = AmmoType.PlasmaCell;
        MagazineSize = 21;
        FireRate = 5.5f;

        Description = "Burst-fire alien rifle that vaporizes targets.";
        Rarity = Rarity.WonderWeapon;
    }
}

public sealed class WunderwaffeDG2 : Gun
{
    public WunderwaffeDG2() : base("Wunderwaffe DG-2")
    {
        Quantity = 1;
        Value = 10_000_000;
        IsSpecial = true;
        IsPurchaseable = false;

        Damage = 10000;
        Range = 50f;
        AmmoType = AmmoType.Arc;
        MagazineSize = 3;
        FireRate = 0.5f;

        Description = "Harness electricity itself. Don’t cross the streams.";
        Rarity = Rarity.WonderWeapon;
    }
}

public sealed class ThunderGun : Gun
{
    public ThunderGun() : base("Thunder Gun")
    {
        Quantity = 1;
        Value = 10_000_000;
        IsSpecial = true;
        IsPurchaseable = false;

        Damage = 10000;
        Range = 25f;
        AmmoType = AmmoType.Air;
        MagazineSize = 4;
        FireRate = 0.25f;

        Description = "Unleashes compressed air blasts that send enemies flying.";
        Rarity = Rarity.WonderWeapon;
    }
}
