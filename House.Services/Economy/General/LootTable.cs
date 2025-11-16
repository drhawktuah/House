using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using House.House.Services.Economy.General;

namespace House.House.Services.Economy.General;

public static class LootTable
{
    public static readonly Dictionary<Rarity, double> RarityWeights = new()
    {
        [Rarity.Common] = 60.0,
        [Rarity.Uncommon] = 25.0,
        [Rarity.Rare] = 10.0,
        [Rarity.Epic] = 4.0,
        [Rarity.Legendary] = 0.9,
        [Rarity.WonderWeapon] = 0.1
    };

    public static Rarity GetRarityFromRoll(double roll)
    {
        if (roll < 0 || roll > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(roll), "Roll must be between 0 and 1");
        }

        double total = RarityWeights.Values.Sum();
        double scaled = roll * total;
        double cumulative = 0.0;

        foreach (var (rarity, weight) in RarityWeights)
        {
            cumulative += weight;

            if (scaled <= cumulative)
            {
                return rarity;
            }
        }

        return Rarity.Common;
    }

    private static double GetSecureDouble()
    {
        Span<byte> bytes = stackalloc byte[8];

        RandomNumberGenerator.Fill(bytes);

        ulong random = BitConverter.ToUInt64(bytes);
        return random / (double) ulong.MaxValue;
    }

    public static string GetDropSummary()
    {
        return string.Join("\n", RarityWeights.Select(kv => $"{kv.Key}: {kv.Value}%"));
    }
}