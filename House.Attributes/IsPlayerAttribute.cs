using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using House.House.Services.Economy;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Attributes;

public sealed class IsPlayerAttribute : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var houseEconomyDatabase = ctx.Services.GetRequiredService<HouseEconomyDatabase>();

        var user = await houseEconomyDatabase.TryGetPlayerAsync(ctx.User.Id);
        return user != null;
    }
}