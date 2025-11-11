using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace House.House.Attributes;

public class IsGuildOwner : CheckBaseAttribute
{
    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        return Task.FromResult(ctx.Guild.Owner == ctx.User);
    }
}