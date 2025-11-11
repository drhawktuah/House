using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using House.House.Core;
using House.House.Services.Database;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Events;

public sealed class MessageDeletedEvent : HouseBotEvent
{
    public MessageDeletedEvent() : base("MessageDeleted")
    {

    }

    public override async Task MainAsync(object sender, EventArgs eventArgs)
    {
        if (sender is not DiscordClient client || eventArgs is not MessageDeleteEventArgs args)
        {
            return;
        }

        var message = args.Message;
        if (message == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return;
        }

        var author = message.Author;

        var commandsNext = client.GetCommandsNext();
        var snipeRepository = commandsNext.Services.GetRequiredService<SnipeRepository>();

        SnipedMessage snipedMessage = new()
        {
            MessageID = message.Id,
            ChannelID = message.Channel.Id,
            AuthorID = author.Id,
            AuthorName = author.Username,
            Content = message.Content,
            DeletedAt = DateTime.UtcNow
        };

        var existing = await snipeRepository.GetLastMessageAsync(message.Channel.Id);

        if (existing != null)
        {
            existing.MessageID = snipedMessage.MessageID;
            existing.AuthorID = snipedMessage.AuthorID;
            existing.AuthorName = snipedMessage.AuthorName;
            existing.Content = snipedMessage.Content;
            existing.DeletedAt = snipedMessage.DeletedAt;

            await snipeRepository.UpdateAsync(existing);
        }
        else
        {
            await snipeRepository.AddAsync(snipedMessage);
        }
    }
}