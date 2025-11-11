using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Items;

public sealed class Medkit : HouseEconomyItem
{
    public Medkit(int quantity = 1)
    {
        ItemName = "Medkit";
        Quantity = quantity;
        Value = 300;
        IsStackable = false;
    }
}

public abstract class Stimulant : HouseEconomyItem
{
    [BsonElement("effect_description")]
    public string EffectDescription { get; set; } = "No effect specified.";

    [BsonElement("duration_seconds")]
    public int DurationSeconds { get; set; } = 0;

    [BsonElement("is_illegal")]
    public bool IsIllegal { get; set; } = false;

    protected Stimulant()
    {
        IsStackable = false;
    }
}

public sealed class CaffeinePill : Stimulant
{
    public CaffeinePill(int quantity = 1)
    {
        ItemName = "Caffeine Pill";
        Quantity = quantity;
        Value = 50;
        DurationSeconds = 300;
        Description = "Increases alertness and reaction time for 5 minutes.";
        IsIllegal = false;
    }
}

public sealed class Vodka : Stimulant
{
    public Vodka(int quantity = 1)
    {
        ItemName = "Vodka";
        Quantity = quantity;
        Value = 35;
        DurationSeconds = 500;
        Description = "Everything's suddenly interesting. You could talk and do anything for hours.";
        IsIllegal = false;
    }
}

public sealed class AdrenalineShot : Stimulant
{
    public AdrenalineShot(int quantity = 1)
    {
        ItemName = "Adrenaline Shot";
        Quantity = quantity;
        Value = 300;
        DurationSeconds = 180;
        EffectDescription = "Boosts speed and damage for a short duration.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Painkillers : Stimulant
{
    public Painkillers(int quantity = 1)
    {
        ItemName = "Painkillers";
        Quantity = quantity;
        Value = 120;
        DurationSeconds = 240;
        EffectDescription = "Reduces damage taken for a short time.";
        IsIllegal = false;
        Rarity = Rarity.Common;
    }
}

public sealed class Cocaine : Stimulant
{
    public Cocaine(int quantity = 1)
    {
        ItemName = "Cocaine";
        Quantity = quantity;
        Value = 2000;
        DurationSeconds = 120;
        EffectDescription = "Extreme energy boost but may cause side effects.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class MarijuanaJoint : Stimulant
{
    public MarijuanaJoint(int quantity = 1)
    {
        ItemName = "Marijuana Joint";
        Quantity = quantity;
        Value = 250;
        DurationSeconds = 420;
        EffectDescription = "Relaxes the user and slightly restores morale.";
        IsIllegal = true;
        Rarity = Rarity.Uncommon;
    }
}

public sealed class PsychedelicMushrooms : Stimulant
{
    public PsychedelicMushrooms(int quantity = 1)
    {
        ItemName = "Psychedelic Mushrooms";
        Quantity = quantity;
        Value = 800;
        DurationSeconds = 600;
        EffectDescription = "Alters perception; may cause visual distortions.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class Methamphetamine : Stimulant
{
    public Methamphetamine(int quantity = 1)
    {
        ItemName = "Methamphetamine";
        Quantity = quantity;
        Value = 5000;
        DurationSeconds = 300;
        EffectDescription = "Increases energy and speed dramatically but with strong side effects.";
        IsIllegal = true;
        Rarity = Rarity.Epic;
    }
}

public sealed class BlueMethamphetamine : Stimulant
{
    public BlueMethamphetamine(int quantity = 1)
    {
        ItemName = "Blue Methamphetamine";
        Quantity = quantity;
        Value = 10000;
        DurationSeconds = 600;
        EffectDescription = "A stronger, purer, and more vibrant high than regular methamphetamine but with superior side effects.";
        IsIllegal = true;
        Rarity = Rarity.Legendary;
    }
}

public sealed class LSD : Stimulant
{
    public LSD(int quantity = 1)
    {
        ItemName = "LSD";
        Quantity = quantity;
        Value = 3500;
        DurationSeconds = 900;
        EffectDescription = "Causes hallucinations and alters perception for 15 minutes.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class ExperimentalSerum : Stimulant
{
    public ExperimentalSerum(int quantity = 1)
    {
        ItemName = "Experimental Serum";
        Quantity = quantity;
        Value = 10000;
        DurationSeconds = 180;
        EffectDescription = "Unpredictable effects, could be very beneficial or harmful.";
        IsIllegal = true;
        Rarity = Rarity.Legendary;
    }
}

public sealed class EnergyDrink : Stimulant
{
    public EnergyDrink(int quantity = 1)
    {
        ItemName = "Energy Drink";
        Quantity = quantity;
        Value = 100;
        DurationSeconds = 120;
        EffectDescription = "Slightly boosts stamina and alertness.";
        IsIllegal = false;
        Rarity = Rarity.Common;
    }
}

public sealed class FocusPill : Stimulant
{
    public FocusPill(int quantity = 1)
    {
        ItemName = "Focus Pill";
        Quantity = quantity;
        Value = 200;
        DurationSeconds = 180;
        EffectDescription = "Improves concentration and aim temporarily.";
        IsIllegal = false;
        Rarity = Rarity.Uncommon;
    }
}

public sealed class Morphine : Stimulant
{
    public Morphine(int quantity = 1)
    {
        ItemName = "Morphine";
        Quantity = quantity;
        Value = 500;
        DurationSeconds = 300;
        EffectDescription = "Painkiller that temporarily restores health and reduces pain effects.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class Oxycodone : Stimulant
{
    public Oxycodone(int quantity = 1)
    {
        ItemName = "Oxycodone";
        Quantity = quantity;
        Value = 1200;
        DurationSeconds = 420;
        EffectDescription = "Strong painkiller; may reduce damage taken temporarily.";
        IsIllegal = true;
        Rarity = Rarity.Rare;
    }
}

public sealed class Heroin : Stimulant
{
    public Heroin(int quantity = 1)
    {
        ItemName = "Heroin";
        Quantity = quantity;
        Value = 5000;
        DurationSeconds = 600;
        EffectDescription = "Very strong effect; temporarily boosts health regen and reduces damage perception, but high addiction risk.";
        IsIllegal = true;
        Rarity = Rarity.Epic;
    }
}

public sealed class Fentanyl : Stimulant
{
    public Fentanyl(int quantity = 1)
    {
        ItemName = "Fentanyl";
        Quantity = quantity;
        Value = 15000;
        DurationSeconds = 300;
        EffectDescription = "Extremely potent opioid; massive boost to health regeneration, very high risk.";
        IsIllegal = true;
        Rarity = Rarity.Legendary;
    }
}

public sealed class Codeine : Stimulant
{
    public Codeine(int quantity = 1)
    {
        ItemName = "Codeine";
        Quantity = quantity;
        Value = 200;
        DurationSeconds = 180;
        EffectDescription = "Mild pain relief and temporary calm effect.";
        IsIllegal = false;
        Rarity = Rarity.Uncommon;
    }
}

public abstract class FoodItem : HouseEconomyItem
{
    [BsonElement("nutrition_value")]
    public int NutritionValue { get; set; } = 0;

    [BsonElement("duration_seconds")]
    public int DurationSeconds { get; set; } = 0; // for temporary buffs

    [BsonElement("effect_description")]
    public string EffectDescription { get; set; } = "No effect.";

    protected FoodItem()
    {
        IsStackable = true;
        IsPurchaseable = true;
        Rarity = Rarity.Common;
    }
}

public sealed class Apple : FoodItem
{
    public Apple(int quantity = 1)
    {
        ItemName = "Apple";
        Quantity = quantity;
        Value = 10;
        NutritionValue = 5;
        EffectDescription = "Restores a small amount of health.";
        Rarity = Rarity.Common;
    }
}

public sealed class Bread : FoodItem
{
    public Bread(int quantity = 1)
    {
        ItemName = "Bread";
        Quantity = quantity;
        Value = 15;
        NutritionValue = 10;
        EffectDescription = "Restores a moderate amount of health.";
        Rarity = Rarity.Common;
    }
}

public sealed class Steak : FoodItem
{
    public Steak(int quantity = 1)
    {
        ItemName = "Steak";
        Quantity = quantity;
        Value = 75;
        NutritionValue = 25;
        EffectDescription = "Restores a significant amount of health.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class ChocolateBar : FoodItem
{
    public ChocolateBar(int quantity = 1)
    {
        ItemName = "Chocolate Bar";
        Quantity = quantity;
        Value = 25;
        NutritionValue = 10;
        EffectDescription = "Restores a small amount of health and morale.";
        Rarity = Rarity.Common;
    }
}

public sealed class Salad : FoodItem
{
    public Salad(int quantity = 1)
    {
        ItemName = "Salad";
        Quantity = quantity;
        Value = 40;
        NutritionValue = 15;
        EffectDescription = "Restores health and slightly increases energy.";
        Rarity = Rarity.Uncommon;
    }
}

public sealed class Burger : FoodItem
{
    public Burger(int quantity = 1)
    {
        ItemName = "Burger";
        Quantity = quantity;
        Value = 100;
        NutritionValue = 30;
        EffectDescription = "Restores a large amount of health and energy.";
        Rarity = Rarity.Rare;
    }
}

public sealed class Sushi : FoodItem
{
    public Sushi(int quantity = 1)
    {
        ItemName = "Sushi";
        Quantity = quantity;
        Value = 150;
        NutritionValue = 35;
        EffectDescription = "Restores a lot of health and slightly boosts morale.";
        Rarity = Rarity.Rare;
    }
}

public sealed class LegendaryFeast : FoodItem
{
    public LegendaryFeast(int quantity = 1)
    {
        ItemName = "Legendary Feast";
        Quantity = quantity;
        Value = 500;
        NutritionValue = 100;
        DurationSeconds = 300;
        EffectDescription = "Massively restores health and gives temporary stat boosts.";
        Rarity = Rarity.Legendary;
    }
}
