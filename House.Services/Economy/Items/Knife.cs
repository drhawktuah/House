using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.General;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Items;

public class Knife : HouseEconomyItem
{
    [BsonElement("is_serrated")]
    public bool IsSerrated { get; set; } = false;

    protected Knife(string itemName) : base(itemName, HouseItemType.MeleeWeapon)
    {
        IsStackable = false;
    }
}

public sealed class Katana : Knife
{
    public Katana(int quantity = 1) : base("Katana")
    {
        Quantity = quantity;
        Value = 250;
        IsSerrated = false;
    }
}

public sealed class Dagger : Knife
{
    public Dagger(int quantity = 1) : base("Dagger")
    {
        Quantity = quantity;
        Value = 75;
        IsSerrated = false;
    }
}

public sealed class BowieKnife : Knife
{
    public BowieKnife(int quantity = 1) : base("Bowie Knife")
    {
        Quantity = quantity;
        Value = 150;
        IsSerrated = true;
    }
}

public sealed class Karambit : Knife
{
    public Karambit(int quantity = 1) : base("Karambit")
    {
        Quantity = quantity;
        Value = 400;
        IsSerrated = true;
    }
}

public sealed class Switchblade : Knife
{
    public Switchblade(int quantity = 1) : base("Switchblade")
    {
        Quantity = quantity;
        Value = 175;
        IsSerrated = false;
    }
}

public sealed class Machete : Knife
{
    public Machete(int quantity = 1) : base("Machete")
    {
        Quantity = quantity;
        Value = 125;
        IsSerrated = false;
    }
}
