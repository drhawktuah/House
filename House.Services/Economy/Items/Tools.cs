using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.General;

namespace House.House.Services.Economy.Items;

public abstract class Tool : HouseEconomyItem
{
    protected Tool(string itemName) : base(itemName, HouseItemType.Tool)
    {
        IsStackable = false;
        IsPurchaseable = true;
    }
}

public sealed class Lockpick : Tool
{
    public Lockpick(int quantity = 1) : base("Lockpick")
    {
        Quantity = quantity;
        Value = 250;
        IsStackable = true;
        Description = "Used to unlock doors or containers without a key.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class Toolkit : Tool
{
    public Toolkit(int quantity = 1) : base("Toolkit")
    {
        Quantity = quantity;
        Value = 500;
        IsStackable = true;
        Description = "A set of tools for repairing and modifying equipment.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Backpack : Tool
{
    public Backpack(int quantity = 1) : base("Backpack")
    {
        Quantity = quantity;
        Value = 750;
        IsStackable = false;
        Description = "Increases carrying capacity for other items.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class HackingDevice : Tool
{
    public HackingDevice(int quantity = 1) : base("Hacking Device")
    {
        Quantity = quantity;
        Value = 2500;
        IsStackable = false;
        Description = "A compact device capable of bypassing digital locks and security systems.";
        Rarity = Rarity.Rare;
    }
}

public sealed class EMPTool : Tool
{
    public EMPTool(int quantity = 1) : base("EMP Tool")
    {
        Quantity = quantity;
        Value = 4500;
        IsStackable = false;
        Description = "Emits a short electromagnetic pulse to disable nearby electronics. Use wisely.";
        Rarity = Rarity.Epic;
    }
}

public sealed class RepairKit : Tool
{
    public RepairKit(int quantity = 1) : base("Repair Kit")
    {
        Quantity = quantity;
        Value = 800;
        IsStackable = true;
        Description = "Used to repair damaged weapons, armor, or gadgets.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class DroneController : Tool
{
    public DroneController(int quantity = 1) : base("Drone Controller")
    {
        Quantity = quantity;
        Value = 7500;
        IsStackable = false;
        Description = "Controls a tactical drone for reconnaissance or delivery purposes.";
        Rarity = Rarity.Legendary;
    }
}

public sealed class SignalJammer : Tool
{
    public SignalJammer(int quantity = 1) : base("Signal Jammer")
    {
        Quantity = quantity;
        Value = 5200;
        IsStackable = false;
        Description = "Blocks wireless signals in a short radius. Illegal in most jurisdictions.";
        Rarity = Rarity.Epic;
    }
}

public sealed class CryptoMiner : Tool
{
    public CryptoMiner(int quantity = 1) : base("Crypto Miner")
    {
        Quantity = quantity;
        Value = 9000;
        IsStackable = true;
        Description = "An illicit, portable miner that generates digital currency over time.";
        Rarity = Rarity.Legendary;
    }
}

public sealed class GrappleGun : Tool
{
    public GrappleGun(int quantity = 1) : base("Grapple Gun")
    {
        Quantity = quantity;
        Value = 3200;
        IsStackable = false;
        Description = "Launch yourself onto rooftops or across gaps like a true action hero.";
        Rarity = Rarity.Rare;
    }
}

public sealed class ThermalCutter : Tool
{
    public ThermalCutter(int quantity = 1) : base("Thermal Cutter")
    {
        Quantity = quantity;
        Value = 2800;
        IsStackable = false;
        Description = "Cuts through metal, safes, and armored doors with precision heat.";
        Rarity = Rarity.Rare;
    }
}