using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using House.House.Services.Economy;

namespace House.House.Attributes;

public sealed class IsPlayerAttribute : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        if (ctx.Services.GetService(typeof(HouseEconomyDatabase)) is not HouseEconomyDatabase houseEconomyDatabase)
        {
            return false;
        }

        var user = await houseEconomyDatabase.GetUserAsync(ctx.User.Id);
        return user != null;
    }
}