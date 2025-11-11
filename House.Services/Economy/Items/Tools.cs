using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Economy.Items;

public sealed class Lockpick : HouseEconomyItem
{
    public Lockpick(int quantity = 1) : base("Lockpick")
    {
        Quantity = quantity;
        Value = 250;
        IsStackable = false;
    }
}

public sealed class Toolkit : HouseEconomyItem
{
    public Toolkit(int quantity = 1) : base("Toolkit")
    {
        Quantity = quantity;
        Value = 500;
        IsStackable = true;
    }
}

public sealed class Backpack : HouseEconomyItem
{
    public Backpack(int quantity = 1) : base("Backpack")
    {
        Quantity = quantity;
        Value = 750;
        IsStackable = true;
    }
}
