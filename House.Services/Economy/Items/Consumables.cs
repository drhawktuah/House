using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Economy.General;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Items;

#region Base Classes

public abstract class Stimulant : HouseEconomyItem
{
    [BsonElement("effect_description")]
    public string EffectDescription { get; set; } = "No effect specified.";

    [BsonElement("duration_seconds")]
    public int DurationSeconds { get; set; } = 0;

    [BsonElement("is_illegal")]
    public bool IsIllegal { get; set; } = false;

    protected Stimulant(string itemName) : base(itemName, HouseItemType.Drug)
    {
        IsStackable = false;
    }
}

public abstract class FoodItem : HouseEconomyItem
{
    [BsonElement("nutrition_value")]
    public int NutritionValue { get; set; } = 0;

    [BsonElement("duration_seconds")]
    public int DurationSeconds { get; set; } = 0;

    [BsonElement("effect_description")]
    public string EffectDescription { get; set; } = "No effect.";

    protected FoodItem(string itemName) : base(itemName, HouseItemType.Food)
    {
        IsStackable = true;
        IsPurchaseable = true;
        Rarity = Rarity.Common;
    }
}

public abstract class MedicalItem : HouseEconomyItem
{
    [BsonElement("healing_amount")]
    public int HealingAmount { get; set; } = 0;

    [BsonElement("effect_description")]
    public string EffectDescription { get; set; } = "Heals health.";

    protected MedicalItem(string itemName) : base(itemName, HouseItemType.Medical)
    {
        IsStackable = false;
        IsPurchaseable = true;
    }
}

#endregion

public sealed class Medkit : MedicalItem
{
    public Medkit(int quantity = 1) : base("Medkit")
    {
        Quantity = quantity;
        Value = 300;
        HealingAmount = 100;
        EffectDescription = "Restores a large portion of health.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class Painkillers : MedicalItem
{
    public Painkillers(int quantity = 1) : base("Painkillers")
    {
        Quantity = quantity;
        Value = 120;
        HealingAmount = 0;
        EffectDescription = "Temporarily reduces pain and damage taken.";
        Rarity = Rarity.Common;
    }
}

public sealed class Morphine : MedicalItem
{
    public Morphine(int quantity = 1) : base("Morphine")
    {
        Quantity = quantity;
        Value = 500;
        HealingAmount = 0;
        EffectDescription = "Strong painkiller; restores health and numbs pain.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Oxycodone : MedicalItem
{
    public Oxycodone(int quantity = 1) : base("Oxycodone")
    {
        Quantity = quantity;
        Value = 1200;
        HealingAmount = 0;
        EffectDescription = "Powerful painkiller; temporarily reduces incoming damage.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Vicodin : MedicalItem
{
    public Vicodin(int quantity = 1) : base("Vicodin")
    {
        Quantity = quantity;
        Value = 4000;
        HealingAmount = 0;
        EffectDescription = "Extreme pain relief; massive intellectual gain.";
        Rarity = Rarity.Legendary;
    }
}

public sealed class CaffeinePill : Stimulant
{
    public CaffeinePill(int quantity = 1) : base("Caffeine Pill")
    {
        Quantity = quantity;
        Value = 50;
        DurationSeconds = 300;
        EffectDescription = "Boosts energy and reaction time for 5 minutes.";
    }
}

public sealed class Vodka : Stimulant
{
    public Vodka(int quantity = 1) : base("Vodka")
    {
        Quantity = quantity;
        Value = 35;
        DurationSeconds = 500;
        EffectDescription = "Boosts morale and confidence temporarily.";
    }
}

public sealed class AdrenalineShot : Stimulant
{
    public AdrenalineShot(int quantity = 1) : base("Adrenaline Shot")
    {
        Quantity = quantity;
        Value = 300;
        DurationSeconds = 180;
        EffectDescription = "Boosts speed and power briefly.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Cocaine : Stimulant
{
    public Cocaine(int quantity = 1) : base("Cocaine")
    {
        Quantity = quantity;
        Value = 2000;
        DurationSeconds = 120;
        EffectDescription = "Massive energy boost with side effects.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class MarijuanaJoint : Stimulant
{
    public MarijuanaJoint(int quantity = 1) : base("Marijuana Joint")
    {
        Quantity = quantity;
        Value = 250;
        DurationSeconds = 420;
        EffectDescription = "Relaxes the user and restores morale slightly.";
        IsIllegal = true;
        Rarity = Rarity.Uncommon;
    }
}

public sealed class PsychedelicMushrooms : Stimulant
{
    public PsychedelicMushrooms(int quantity = 1) : base("Psychedelic Mushrooms")
    {
        Quantity = quantity;
        Value = 800;
        DurationSeconds = 600;
        EffectDescription = "Alters perception and causes vivid hallucinations.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class Methamphetamine : Stimulant
{
    public Methamphetamine(int quantity = 1) : base("Methamphetamine")
    {
        Quantity = quantity;
        Value = 5000;
        DurationSeconds = 300;
        EffectDescription = "Increases energy and focus drastically.";
        IsIllegal = true;
        Rarity = Rarity.Epic;
    }
}

public sealed class BlueMethamphetamine : Stimulant
{
    public BlueMethamphetamine(int quantity = 1) : base("Blue Methamphetamine")
    {
        Quantity = quantity;
        Value = 10000;
        DurationSeconds = 600;
        EffectDescription = "Highly pure formula; extreme power, extreme risk.";
        IsIllegal = true;
        Rarity = Rarity.Legendary;
    }
}

public sealed class LSD : Stimulant
{
    public LSD(int quantity = 1) : base("LSD")
    {
        Quantity = quantity;
        Value = 3500;
        DurationSeconds = 900;
        EffectDescription = "Hallucinogenic; distorts perception for 15 minutes.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class Heroin : Stimulant
{
    public Heroin(int quantity = 1) : base("Heroin")
    {
        Quantity = quantity;
        Value = 5000;
        DurationSeconds = 600;
        EffectDescription = "Boosts health regen; extremely addictive.";
        IsIllegal = true;
        Rarity = Rarity.Epic;
    }
}

public sealed class Fentanyl : Stimulant
{
    public Fentanyl(int quantity = 1) : base("Fentanyl")
    {
        Quantity = quantity;
        Value = 15000;
        DurationSeconds = 300;
        EffectDescription = "Extremely potent; massive health regen boost, lethal risk.";
        IsIllegal = true;
        Rarity = Rarity.Legendary;
    }
}

public sealed class Codeine : Stimulant
{
    public Codeine(int quantity = 1) : base("Codeine")
    {
        Quantity = quantity;
        Value = 200;
        DurationSeconds = 180;
        EffectDescription = "Mild pain relief and calmness.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class ExperimentalSerum : Stimulant
{
    public ExperimentalSerum(int quantity = 1) : base("Experimental Serum")
    {
        Quantity = quantity;
        Value = 10000;
        DurationSeconds = 180;
        EffectDescription = "Unpredictable outcome: may enhance or harm.";
        Rarity = Rarity.Legendary;
        IsIllegal = true;
    }
}

public sealed class EnergyDrink : Stimulant
{
    public EnergyDrink(int quantity = 1) : base("Energy Drink")
    {
        Quantity = quantity;
        Value = 100;
        DurationSeconds = 120;
        EffectDescription = "Small stamina and alertness boost.";
        Rarity = Rarity.Common;
    }
}

public sealed class FocusPill : Stimulant
{
    public FocusPill(int quantity = 1) : base("Focus Pill")
    {
        Quantity = quantity;
        Value = 200;
        DurationSeconds = 180;
        EffectDescription = "Improves focus and accuracy temporarily.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class Apple : FoodItem
{
    public Apple(int quantity = 1) : base("Apple")
    {
        Quantity = quantity;
        Value = 10;
        NutritionValue = 5;
        EffectDescription = "Restores a small amount of health.";
    }
}

public sealed class Bread : FoodItem
{
    public Bread(int quantity = 1) : base("Bread")
    {
        Quantity = quantity;
        Value = 15;
        NutritionValue = 10;
        EffectDescription = "Restores moderate health.";
    }
}

public sealed class Steak : FoodItem
{
    public Steak(int quantity = 1) : base("Steak")
    {
        Quantity = quantity;
        Value = 75;
        NutritionValue = 25;
        EffectDescription = "Restores significant health.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class ChocolateBar : FoodItem
{
    public ChocolateBar(int quantity = 1) : base("Chocolate Bar")
    {
        Quantity = quantity;
        Value = 25;
        NutritionValue = 10;
        EffectDescription = "Restores some health and morale.";
    }
}

public sealed class Salad : FoodItem
{
    public Salad(int quantity = 1) : base("Salad")
    {
        Quantity = quantity;
        Value = 40;
        NutritionValue = 15;
        EffectDescription = "Restores health and boosts energy slightly.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class Burger : FoodItem
{
    public Burger(int quantity = 1) : base("Burger")
    {
        Quantity = quantity;
        Value = 100;
        NutritionValue = 30;
        EffectDescription = "Restores lots of health and energy.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Sushi : FoodItem
{
    public Sushi(int quantity = 1) : base("Sushi")
    {
        Quantity = quantity;
        Value = 150;
        NutritionValue = 35;
        EffectDescription = "Restores health and boosts morale.";
        Rarity = Rarity.Rare;
    }
}

public sealed class LegendaryFeast : FoodItem
{
    public LegendaryFeast(int quantity = 1) : base("Legendary Feast")
    {
        Quantity = quantity;
        Value = 500;
        NutritionValue = 100;
        DurationSeconds = 300;
        EffectDescription = "Fully restores health and grants buffs.";
        Rarity = Rarity.Legendary;
    }
}
