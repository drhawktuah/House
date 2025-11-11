using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
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

        if (args.User.IsBot)
        {
            return;
        }

        var guild = args.Guild;
        var message = args.Message;

        if (guild is null || message is null)
        {
            return;
        }

        var commandsNext = client.GetCommandsNext();
        var guildRepository = commandsNext.Services.GetRequiredService<GuildRepository>();
        var starboardRepository = commandsNext.Services.GetRequiredService<StarboardRepository>();

        var databaseGuild = await guildRepository.TryGetAsync(guild.Id);
        if (databaseGuild is null || !databaseGuild.StarboardChannelID.HasValue)
        {
            return;
        }

        var starboardChannel = guild.GetChannel(databaseGuild.StarboardChannelID.Value) ?? guild.SystemChannel;
        if (starboardChannel is null)
        {
            return;
        }

        var starEmoji = DiscordEmoji.FromUnicode("⭐");
        var starReaction = message.Reactions.FirstOrDefault(r => r.Emoji.GetDiscordName() == starEmoji.GetDiscordName());

        int threshold = 2;

        if (starReaction == null || starReaction.Count < threshold)
        {
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("⭐ Starboard")
            .WithDescription(string.IsNullOrWhiteSpace(message.Content) ? "*No message content*" : message.Content)
            .WithAuthor(message.Author.Username, null, message.Author.AvatarUrl)
            .WithFooter($"⭐ {starReaction.Count} stars • Message ID: {message.Id}", client.CurrentUser.AvatarUrl)
            .WithTimestamp(message.Timestamp)
            .WithColor(DiscordColor.Gold)
            .WithUrl($"https://discord.com/channels/{guild.Id}/{message.Channel.Id}/{message.Id}");

        if (message.Attachments.Count > 0)
        {
            var attachment = message.Attachments[0];
            if (attachment.Url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                attachment.Url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                attachment.Url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                attachment.Url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                attachment.Url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                embed.WithImageUrl(attachment.Url);
            }
        }

        var existingEntry = await starboardRepository.TryGetAsync(message.Id);

        if (existingEntry != null)
        {
            try
            {
                var existingMessage = await starboardChannel.GetMessageAsync(existingEntry.StarboardMessageID);
                if (existingMessage != null)
                {
                    DiscordMessageBuilder messageBuilder = new()
                    {
                        Embed = embed
                    };

                    await existingMessage.ModifyAsync(messageBuilder);
                    return;
                }
            }
            catch (NotFoundException)
            {
                await starboardRepository.DeleteAsync(existingEntry.ID);
            }
        }

        var starboardMessage = await starboardChannel.SendMessageAsync(embed);

        StarboardEntry entry = new()
        {
            MessageID = message.Id,
            GuildID = guild.Id,
            StarboardMessageID = starboardMessage.Id,
            CreatedAt = DateTime.UtcNow
        };

        await starboardRepository.AddAsync(entry);
    }
}