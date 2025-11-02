using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using House.House.Core;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Attributes;

/// <summary>
/// Determines if a user is the bot owner
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IsOwnerAttribute : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var config = ctx.Services.GetRequiredService<Config>();
        var owners = config.OwnerIDS;

        return Task.FromResult(owners.Any(x => x == ctx.User.Id));
    }
}
