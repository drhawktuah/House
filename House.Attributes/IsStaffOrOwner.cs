using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using House.House.Core;
using House.House.Services.Database;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Attributes;

public sealed class IsStaffOrOwnerAttribute : CheckBaseAttribute
{
    private readonly Position minimumPosition;

    public IsStaffOrOwnerAttribute(Position minimumPosition = Position.Moderator)
    {
        this.minimumPosition = minimumPosition;
    }

    public override async Task<bool> ExecuteCheckAsync(CommandContext context, bool help)
    {
        Config config = context.Services.GetRequiredService<Config>();
        StaffUserRepository userRepository = context.Services.GetRequiredService<StaffUserRepository>();

        bool isAllowed = false;

        if (config.OwnerIDS.Any(x => x == context.User.Id))
        {
            isAllowed = true;
        }

        StaffUser? staffUser = await userRepository.TryGetAsync(context.User.Id);

        if (staffUser is not null && staffUser.Position <= minimumPosition)
        {
            isAllowed = true;
        }

        return isAllowed;
    }
}