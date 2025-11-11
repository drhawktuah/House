using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace House.House.Utils;

public static class EconomyUtils
{
    public static string FormatCurrency(long amount)
    {
        bool isNegative = amount < 0;
        ulong absAmount = (ulong)(isNegative ? -amount : amount);

        string formatted;

        if (absAmount >= 1_000_000_000)
        {
            formatted = $"{absAmount / 1_000_000_000.0:F1}B";
        }
        else if (absAmount >= 1_000_000)
        {
            formatted = $"{absAmount / 1_000_000.0:F1}M";
        }
        else if (absAmount >= 1_000)
        {
            formatted = $"{absAmount / 1_000.0:F1}K";
        }
        else
        {
            formatted = absAmount.ToString("N0");
        }

        return isNegative ? "-" + formatted : formatted;
    }

    public static long CalculateTokensFromVicodin(int vicodinAmount)
    {
        const int TokensPerVicodin = 10_000;

        return vicodinAmount * TokensPerVicodin;
    }

    public static List<DiscordActionRowComponent> BuildGameGrid(
        int buttonRows,
        int buttonColumns,

        string defaultLabel,
        string activeLabel,
        string foundLabel,

        int activeIndex = -1,
        int? foundIndex = null,

        bool disableAll = false
    )
    {
        List<DiscordActionRowComponent> components = [];

        for (int row = 0; row < buttonRows; row++)
        {
            List<DiscordComponent> rowButtons = [];

            for (int col = 0; col < buttonColumns; col++)
            {
                int index = row * buttonColumns + col;

                string label = defaultLabel;
                bool disabled = true;

                if (foundIndex.HasValue && foundIndex.Value == index)
                {
                    label = foundLabel;
                }
                else if (!disableAll && index == activeIndex)
                {
                    label = activeLabel;
                    disabled = false;
                }

                DiscordButtonComponent item = new(
                    ButtonStyle.Primary,
                    customId: $"btn_{index}",
                    label: label,
                    disabled: disabled
                );

                rowButtons.Add(item);
            }

            components.Add(new DiscordActionRowComponent(rowButtons));
        }

        return components;
    }
}