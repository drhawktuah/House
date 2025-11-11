using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using House.House.Services.Database;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IsStaffAttribute : CheckBaseAttribute
{
    private readonly Position minimumPosition;

    public IsStaffAttribute(Position minimumPosition = Position.Moderator)
    {
        this.minimumPosition = minimumPosition;
    }
    public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        var resp = ctx.Services.GetRequiredService<StaffUserRepository>();
        if(resp == null)
        {
            return false;
        }

        var staffUser = await resp.GetAsync(ctx.User.Id);
        if (staffUser == null)
        {
            return false;
        }

        return staffUser.Position <= minimumPosition;
    }
}