using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using House.House.Core;
using House.House.Services.Fuzzy;
using House.House.Services.Protection;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Events;

public sealed class MessageCreatedEvent : HouseBotEvent
{
    public MessageCreatedEvent() : base("MessageCreated")
    {
    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not DiscordClient client || eventArgs is not MessageCreateEventArgs args)
        {
            return;
        }

        var author = args.Author;
        if (author.IsBot)
        {
            return;
        }

        var message = args.Message;
        if (message.MentionedUsers.Any(u => u == client.CurrentUser))
        {
            string[] replies = [
                "It's never lupus",
                "Hi!",
                "Yes, hello!",
                "Cuddy = Katie",
                "VICODIN."
            ];

            int index = (BitConverter.ToInt32(RandomNumberGenerator.GetBytes(4), 0) & int.MaxValue) % replies.Length;

            await message.RespondAsync(replies[index]);
            return;
        }

        var commandsNext = client.GetCommandsNext();

        var antiNukeService = commandsNext.Services.GetRequiredService<AntiNukeService>();

        await antiNukeService.HandleMessageAsync(message);

        var fuzzyService = commandsNext.Services.GetRequiredService<HouseFuzzyMatchingService>();
        var config = commandsNext.Services.GetRequiredService<Config>();

        var (messagePosition, matchedPrefix) = FindMatchingPrefixes(config, message);
        if (messagePosition == -1 || matchedPrefix == null)
        {
            return;
        }

        var commandString = message.Content[messagePosition..].Trim();

        if (string.IsNullOrWhiteSpace(commandString))
        {
            return;
        }

        var parts = commandString.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var (commandName, rawArguments) = (parts.Length > 0 ? parts[0] : string.Empty, parts.Length > 1 ? parts[1] : string.Empty);

        var command = commandsNext.FindCommand(commandName, out var _);
        if (command == null)
        {
            //var results = fuzzyService.GetResults(commandName, 50, mode: ONFQ.ONFQ.Models.BlendMode.BlendAll);
            var results = fuzzyService.GetResults(commandName);
            var embed = await fuzzyService.ToDiscordEmbed(commandName, results);

            await message.Channel.SendMessageAsync(embed);
            return;
        }

        var context = commandsNext.CreateContext(message, matchedPrefix, command, rawArguments);
        await commandsNext.ExecuteCommandAsync(context);
    }

    private static (int, string?) FindMatchingPrefixes(Config config, DiscordMessage message)
    {
        foreach (var prefix in config.DefaultPrefixes)
        {
            int index = message.GetStringPrefixLength(prefix);
            if (index != -1)
            {
                return (index, prefix);
            }
        }

        return (-1, null);
    }
}