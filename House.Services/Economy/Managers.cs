using House.House.Services.Economy.General;
using House.House.Services.Economy.Items;
using House.House.Services.Economy.Vendors;

namespace House.House.Services.Economy;

public static class VendorManager
{
    public static IReadOnlyList<HouseEconomyVendor> Vendors => VendorPresets.VendorPool;

    public static HouseEconomyVendor? Find(string name)
    {
        HouseEconomyVendor? economyVendor = null;

        foreach (var vendor in Vendors)
        {
            if (vendor.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || vendor.Aliases.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                economyVendor = vendor;
                break;
            }
        }

        return economyVendor;
    }
}

public static class ItemManager
{
    public static IReadOnlyList<HouseEconomyItem> Items => GlobalItemPool.AllItems;

    public static HouseEconomyItem? Find(string name)
    {
        HouseEconomyItem? economyItem = null;

        foreach (var item in Items)
        {
            if (item.ItemName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                economyItem = item;
                break;
            }
        }

        return economyItem;
    }
}