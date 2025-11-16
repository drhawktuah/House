using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using House.House.Services.Economy;
using House.House.Services.Economy.General;

namespace House.House.Extensions;

public static class RarityExtensions
{
    public static string GetEmoji(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "⚪",
            Rarity.Uncommon => "🟢",
            Rarity.Rare => "🔵",
            Rarity.Epic => "🟣",
            Rarity.Legendary => "🟡",
            Rarity.WonderWeapon => "🔴",
            _ => "⚫"
        };
    }

    public static DiscordColor GetDiscordColor(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => new DiscordColor(200, 200, 200),    // Gray
            Rarity.Uncommon => new DiscordColor(0, 200, 0),      // Green
            Rarity.Rare => new DiscordColor(0, 112, 221),        // Blue
            Rarity.Epic => new DiscordColor(163, 53, 238),       // Purple
            Rarity.Legendary => new DiscordColor(255, 204, 0),   // Gold
            Rarity.WonderWeapon => new DiscordColor(255, 0, 0),  // Red
            _ => DiscordColor.Black
        };
    }

    public static string GetDisplayName(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "Common",
            Rarity.Uncommon => "Uncommon",
            Rarity.Rare => "Rare",
            Rarity.Epic => "Epic",
            Rarity.Legendary => "Legendary",
            Rarity.WonderWeapon => "Wonder Weapon",
            _ => "Unknown"
        };
    }
}