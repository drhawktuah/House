using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using House.House.Services.Database;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class IsConfigExisting : CheckBaseAttribute
{
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var guildRepo = ctx.Services.GetService<GuildRepository>();
        if (guildRepo == null)
        {
            return false;
        }

        if (ctx.Guild == null)
        {
            return false;
        }

        var config = await guildRepo.TryGetAsync(ctx.Guild.Id);

        return config == null;
    }

    public override string ToString() => "A guild configuration already exists for this server";
}