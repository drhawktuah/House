using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Items;

public class Knife : HouseEconomyItem
{
    [BsonElement("is_serrated")]
    public required bool IsSerrated { get; set; } = false;
}

public sealed class Katana : Knife
{
    public Katana(int quantity = 1)
    {
        ItemName = "Katana";
        Quantity = quantity;
        Value = 250;
        IsSingularItem = false;
    }
}

public sealed class Dagger : Knife
{
    public Dagger(int quantity = 1)
    {
        ItemName = "Dagger";
        Quantity = quantity;
        Value = 75;
        IsSingularItem = false;
        IsSerrated = false;
    }
}

public sealed class BowieKnife : Knife
{
    public BowieKnife(int quantity = 1)
    {
        ItemName = "Bowie Knife";
        Quantity = quantity;
        Value = 150;
        IsSingularItem = false;
        IsSerrated = true;
    }
}

public sealed class Karambit : Knife
{
    public Karambit(int quantity = 1)
    {
        ItemName = "Karambit";
        Quantity = quantity;
        Value = 400;
        IsSingularItem = false;
    }
}

public sealed class Switchblade : Knife
{
    public Switchblade(int quantity = 1)
    {
        ItemName = "Switchblade";
        Quantity = quantity;
        Value = 175;
        IsSingularItem = false;
    }
}

public sealed class Machete : Knife
{
    public Machete(int quantity = 1)
    {
        ItemName = "Machete";
        Quantity = quantity;
        Value = 125;
        IsSingularItem = false;
    }
}