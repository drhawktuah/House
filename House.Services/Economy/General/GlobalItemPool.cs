using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.Items;
using House.House.Services.Economy.General;
using House.House.Services.Economy.Vendors;

namespace House.House.Services.Economy.General;

public static class GlobalItemPool
{
    public static readonly List<HouseEconomyItem> AllItems = new()
    {
        // Guns
        new Handgun(),
        new Shotgun(),
        new SniperRifle(),
        new AssaultRifle(),
        new LMG(),
        new Crossbow(),
        new RayGun(),
        new RayGunMarkII(),
        new WunderwaffeDG2(),
        new ThunderGun(),

        // Medical
        new Medkit(),
        new Painkillers(),
        new Morphine(),
        new ExperimentalSerum(),
        new Fentanyl(),
        new Vicodin(),

        // Drugs
        new CaffeinePill(),
        new AdrenalineShot(),
        new Cocaine(),
        new LSD(),
        new Methamphetamine(),
        new BlueMethamphetamine(),
        new Heroin(),

        // Food
        new Apple(),
        new Bread(),
        new Salad(),
        new Burger(),
        new Sushi(),
        new Steak(),
        new LegendaryFeast(),

        // Tools
        new Lockpick(),
        new Toolkit(),
        new Backpack(),

        // Knives
        new Katana(),
        new Dagger(),
        new BowieKnife(),
        new Karambit(),
        new Switchblade()
    };

    public static IEnumerable<HouseEconomyItem> FilterForVendor(VendorType type)
    {
        return AllItems.Where(item => type switch
        {
            VendorType.Weapon => item is Gun,
            VendorType.BlackMarket => item is Gun gun && (gun.IsSpecial || gun.Rarity >= Rarity.Legendary),
            VendorType.Medical => item is MedicalItem,
            VendorType.DrugDealer => item is Stimulant,
            VendorType.Food => item is FoodItem,
            VendorType.Tool => item is Tool,
            VendorType.General => true && item.Rarity != Rarity.WonderWeapon && item.Rarity != Rarity.Legendary,
            _ => false,
        });
    }
}
