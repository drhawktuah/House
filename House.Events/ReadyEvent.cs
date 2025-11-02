using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using House.House.Core;

namespace House.House.Events;

public sealed class ReadyEvent : HouseBotEvent
{
    public ReadyEvent() : base("Ready")
    {
        
    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not DiscordClient client)
        {
            return;
        }

        Console.WriteLine($"{client.CurrentUser.Username} is ready");

        DiscordActivity activity = new()
        {
            ActivityType = ActivityType.Watching,
            Name = "House's server"
        };

        await client.UpdateStatusAsync(activity, UserStatus.Idle);
    }
}