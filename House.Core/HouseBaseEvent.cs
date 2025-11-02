using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;

namespace House.House.Core;

public abstract class HouseBaseEvent
{
    public string Name { get; }

    protected HouseBaseEvent(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public abstract Task MainAsync(object sender, EventArgs args);
}

public class HouseCommandsNextEvent : HouseBaseEvent
{
    public HouseCommandsNextEvent(string name) : base(name)
    {
    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not DiscordClient client || eventArgs is not CommandEventArgs args)
        {
            return;
        }

        await Task.CompletedTask;
    }
}

public class HouseBotEvent : HouseBaseEvent
{
    public HouseBotEvent(string name) : base(name)
    {
    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not DiscordClient client || eventArgs is not DiscordEventArgs args)
        {
            return;
        }

        await Task.CompletedTask;
    }
}
