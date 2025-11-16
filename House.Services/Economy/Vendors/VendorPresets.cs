using House.House.Services.Economy.Items;
using House.House.Services.Economy.General;

namespace House.House.Services.Economy.Vendors;

public static class VendorPresets
{
    public static HouseEconomyVendor GunVendor => new()
    {
        Name = "Eric Foreman",
        Aliases = ["Eric", "Foreman", "Dr. Foreman", "Control Freak", "Black Guy", "Black", "Black Cuddy"],
        Type = VendorType.Weapon,
        MarkupRate = 1.2,
        Description = "Disciplined, serious, and a bit of a control freak. Foreman ensures everything is in order—just don't question his pricing.",
        Inventory =
        [
            new Handgun(),
            new Shotgun(),
            new SniperRifle(),
            new Crossbow()
        ],
        Quirk = "Foreman inspects every purchase carefully. Occasionally gives a 5% discount for polite customers.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor SpecialtyGunVendor => new()
    {
        Name = "Lawrence Kutner",
        Type = VendorType.BlackMarket,
        Aliases = ["Lawrence", "Kutner", "Indian", "Golly", "Joyous", "Dr. Kutner"],
        MarkupRate = 2.0,
        Description = "Excitable, unpredictable, and loves extreme firepower. Buying from him is always a gamble.",
        Inventory =
        [
            new RayGun(),
            new RayGunMarkII(),
            new WunderwaffeDG2(),
            new ThunderGun()
        ],
        Quirk = "Sometimes randomly discounts a rare weapon—or sells you something you didn't ask for.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor MedicalVendor => new()
    {
        Name = "Gregory House",
        Aliases = ["Greg", "House", "Gregory", "Gregory House", "Dr. House", "Vicodin", "Cane"],
        Type = VendorType.Medical,
        MarkupRate = 1.5,
        Description = "Sarcastic, brilliant, and unpredictable. Only the desperate buy from House—but the effects are always... effective.",
        Inventory =
        [
            new Medkit(),
            new Painkillers(),
            new Morphine(),
            new ExperimentalSerum(),
            new Fentanyl(),
            new Vicodin()
        ],
        Quirk = "House might insult your intelligence, but grants a 10% bonus effect to any medical item purchased.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor DrugVendor => new()
    {
        Name = "Robert Chase",
        Aliases = ["Robert", "Chase", "Dr. Robert Chase", "Dr. Chase", "House Suckup", "Child Lover", "Kisser Of The 9's"],
        Type = VendorType.DrugDealer,
        MarkupRate = 1.3,
        Description = "Charming and manipulative. Chase knows what you want before you even ask.",
        Inventory =
        [
            new CaffeinePill(),
            new AdrenalineShot(),
            new Cocaine(),
            new LSD(),
            new BlueMethamphetamine()
        ],
        Quirk = "Chase flirts; sometimes gives +1 free quantity of stimulant items.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor FoodVendor => new()
    {
        Name = "Allison Cameron",
        Aliases = ["Allison", "Cameron", "Dr. Cameron", "Dead Husband", "Dead Husband Lover", "House Lover"],
        Type = VendorType.Food,
        MarkupRate = 1.1,
        Description = "Caring and considerate—she always makes sure you eat something healthy... sometimes.",
        Inventory =
        [
            new Apple(),
            new Bread(),
            new Salad(),
            new Burger(),
            new Sushi(),
            new LegendaryFeast()
        ],
        Quirk = "Cameron reminds you to eat well—each purchase restores +5 temporary morale.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor KnifeVendor => new()
    {
        Name = "James Wilson",
        Aliases = ["James", "Wilson", "Dr. Wilson", "Cheater", "Doormat"],
        Type = VendorType.Weapon,
        MarkupRate = 1.0,
        Description = "Soft-spoken and diplomatic. He prefers knives for precise work rather than chaos.",
        Inventory =
        [
            new Katana(),
            new Dagger(),
            new BowieKnife(),
            new Karambit(),
            new Switchblade()
        ],
        Quirk = "Wilson calmly explains the best usage of knives; every purchase increases critical strike chance by 2%.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor TechVendor => new()
    {
        Name = "Chris Taub",
        Aliases = ["Chris", "Taub", "Dr. Taub", "Cheater 2", "Tiny", "Pinsized", "Elf"],
        Type = VendorType.Tool,
        MarkupRate = 1.2,
        Description = "Resourceful and practical, Taub always has tools and gadgets to get you out of trouble.",
        Inventory =
        [
            new Lockpick(),
            new Toolkit(),
            new Backpack()
        ],
        Quirk = "Taub offers a 5% discount if you explain how the tool will be used.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor ExoticDrugVendor => new()
    {
        Name = "Remy 'Thirteen' Hadley",
        Aliases = ["Remy", "Thirtreen", "Hadley", "Dr. Hadley", "Lesbian"],
        Type = VendorType.DrugDealer,
        MarkupRate = 1.4,
        Description = "Mysterious and edgy. You never know exactly what she'll have, but it's usually potent.",
        Inventory =
        [
            new Methamphetamine(),
            new BlueMethamphetamine(),
            new Heroin(),
            new Fentanyl()
        ],
        Quirk = "Thirteen keeps a poker face; 10% chance for items to have double duration.",
        LastRestockTime = DateTime.MinValue
    };

    public static HouseEconomyVendor SpecialtyFoodVendor => new()
    {
        Name = "Lisa Cuddy",
        Aliases = ["Lisa", "Cuddy", "Dr. Cuddy", "Big-backed", "Big Backed", "Narcissist"],
        Type = VendorType.Food,
        MarkupRate = 1.3,
        Description = "Organized and classy. Offers premium and healthy food options with a touch of luxury.",
        Inventory =
        [
            new LegendaryFeast(),
            new Steak(),
            new Sushi(),
            new Salad()
        ],
        Quirk = "Cuddy's organized service ensures food restores 5% more health than usual.",
        LastRestockTime = DateTime.MinValue
    };

    public static readonly List<HouseEconomyVendor> VendorPool =
    [
        GunVendor,
        SpecialtyGunVendor,
        MedicalVendor,
        DrugVendor,
        FoodVendor,
        KnifeVendor,
        TechVendor,
        ExoticDrugVendor,
        SpecialtyFoodVendor
    ];
}
