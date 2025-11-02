using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using House.House.Attributes;

namespace House.House.Modules;

public sealed class TestModule : BaseCommandModule
{
    [Command("test")]
    [IsOwner]
    public async Task TestAsync(CommandContext context)
    {
        await context.Channel.SendMessageAsync(BuildEmbeds());
    }

    private static DiscordMessageBuilder BuildEmbeds()
    {
        DiscordMessageBuilder messageBuilder = new();

        DiscordEmbedBuilder embedBuilder_1 = new()
        {
            Title = "first",
            Description = "content 1"
        };

        DiscordEmbedBuilder embedBuilder_2 = new()
        {
            Title = "second",
            Description = "content 2"
        };

        DiscordEmbedBuilder embedBuilder_3 = new()
        {
            Title = "third",
            Description = "content 3"
        };

        List<DiscordEmbed> embedBuilders = [
            embedBuilder_1,
            embedBuilder_2,
            embedBuilder_3
        ];

        messageBuilder.AddEmbeds(embedBuilders);

        return messageBuilder;
    }
}