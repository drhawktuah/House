using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using House.House.Core;
using House.House.Services.Database;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Events;

public sealed class MessageReactionAddedEvent : HouseBotEvent
{
    public MessageReactionAddedEvent() : base("MessageReactionAdded")
    {
    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not DiscordClient client || eventArgs is not MessageReactionAddEventArgs args)
        {
            return;
        }

        var guild = args.Guild;
        var message = args.Message;

        var commandsNext = client.GetCommandsNext();

        var guildRepository = commandsNext.Services.GetRequiredService<GuildRepository>();
        var found = await guildRepository.TryGetAsync(guild.Id);

        if (found is null || !found.StarboardChannelID.HasValue)
        {
            return;
        }

        DiscordChannel? starboardChannel = null;

        if (found != null && found.StarboardChannelID.HasValue)
        {
            starboardChannel = guild.GetChannel(found.StarboardChannelID.Value);
        }

        if (starboardChannel == null)
        {
            starboardChannel = guild.SystemChannel;
        }

        if (starboardChannel == null)
        {
            return;
        }

        DiscordEmoji starEmoji = DiscordEmoji.FromUnicode("⭐");
        DiscordReaction? starReaction = message.Reactions.FirstOrDefault(r => r.Emoji == starEmoji);

        if (starReaction != null && starReaction.Count >= 2)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Starboard",
                Description = string.IsNullOrWhiteSpace(message.Content) ? "No message content" : message.Content
            };

            embedBuilder.WithAuthor(message.Author.Username, null, message.Author.AvatarUrl);
            embedBuilder.WithFooter($"⭐ {starReaction.Count} starts | ID: {message.Id}", client.CurrentUser.AvatarUrl);
            embedBuilder.WithTimestamp(message.Timestamp);
            embedBuilder.WithColor(DiscordColor.Gold);
            embedBuilder.WithUrl($"https://discord.com/channels/{guild.Id}/{message.Channel.Id}/{message.Id}");

            if (message.Attachments.Any())
            {
                DiscordAttachment attachment = message.Attachments[0];

                embedBuilder.WithImageUrl(attachment.Url);
            }

            await starboardChannel.SendMessageAsync(embedBuilder);
        }
    }
}