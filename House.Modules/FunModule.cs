using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using House.House.Extensions;
using House.House.Services.Database;
using House.House.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Modules;

[Description("House's fun module")]
public sealed class FunModule : BaseCommandModule
{
    [Command("masktext")]
    [Aliases("mask")]
    [Description("Masks text with '*' identifier")]
    [Cooldown(2, 8, CooldownBucketType.User)]
    [RequireGuild]
    public async Task MaskStringAsync(CommandContext context, [RemainingText] string text)
    {
        string masked = text.MaskString();

        await context.RespondAsync($"Original text: `{text}`\nMasked text: `{masked}`");
    }

    [Command("fliptext")]
    [Aliases("flip")]
    [Description("Flips text upside-down like cool sigma phonk")]
    [Cooldown(2, 8, CooldownBucketType.User)]
    [RequireGuild]
    public async Task FlipTextAsync(CommandContext context, [RemainingText] string text)
    {
        string flipped = text.FlipString();

        await context.RespondAsync($"Flipped: `{flipped}`");
    }

    [Command("mock")]
    [Aliases("mockuser", "sarcasticmock")]
    [Description("Sarcastically mocks a user, dumbifying their sentence")]
    [Cooldown(2, 5, CooldownBucketType.User)]
    [RequireGuild]
    public async Task MockUserAsync(CommandContext context, DiscordMember member)
    {
        var messages = await context.Channel.GetMessagesAsync(limit: 100);
        var lastMessage = messages.FirstOrDefault(m => m.Author == member);

        if (lastMessage == null)
        {
            await context.RespondAsync($"No recent message found from {member.Username} in this chat");
            return;
        }

        if(string.IsNullOrWhiteSpace(lastMessage.Content))
        {
            await context.RespondAsync($"No recent message found from {member.Username} in this chat");
            return;
        }

        string newText = "";

        for (int i = 0; i < lastMessage.Content.Length; i++)
        {
            if ((i + 1) % 2 == 0)
            {
                newText += lastMessage.Content[i].ToString().ToUpperInvariant();
            }
            else
            {
                newText += lastMessage.Content[i];
            }
        }

        await context.RespondAsync(newText);
        return;
    }

    /*
    public async Task MockUserAsync(CommandContext context, [RemainingText] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await context.RespondAsync("Text cannot be empty");
            return;
        }

        text = text.ToLowerInvariant();

        string newText = "";

        for (int i = 0; i < text.Length; i++)
        {
            if ((i + 1) % 2 == 0)
            {
                newText += text[i].ToString().ToUpperInvariant();
            }
            else
            {
                newText += text[i];
            }
        }

        await context.RespondAsync(newText);
        return;
    }
    */

    [Command("evan")]
    [Aliases("butters", "evanfowler", "fowler", "diaper", "diapy", "diap", "diapman", "diaperfowler")]
    [Description("Gives you a random attachment of Mr. Butters!")]
    [Cooldown(2, 30, CooldownBucketType.Global)]
    [RequireGuild]
    public async Task GetEvanPicture(CommandContext context)
    {
        var evanChannel = await context.Client.GetChannelAsync(1425349262799015936);
        var pinnedMessages = await evanChannel.GetPinnedMessagesAsync();
        var attachments = pinnedMessages.SelectMany(m => m.Attachments).ToList();

        DiscordAttachment? attachment = null;

        if (pinnedMessages.Count > 0)
        {
            var indexBytes = new byte[4];
            RandomNumberGenerator.Fill(indexBytes);
            int index = BitConverter.ToInt32(indexBytes, 0) & int.MaxValue;
            index %= pinnedMessages.Count;

            attachment = attachments[index];
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = "Butters",
            Color = EmbedUtils.EmbedColor
        };

        if (attachment is not null)
        {
            embedBuilder.WithImageUrl(attachment.Url);
        }

        await context.RespondAsync(embedBuilder);
    }

    [Command("dice")]
    [Aliases("diceroll")]
    [Cooldown(1, 15, CooldownBucketType.User)]
    public async Task RollDiceAsync(CommandContext context, params string[] choices)
    {
        if (choices.Length < 2)
        {
            await context.RespondAsync("`the amount of dice choices must be more than or equal to 2`");
            return;
        }

        var options = choices
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList();

        if (options.Count < 2)
        {
            await context.RespondAsync("`the amount of dice choices must be more than or equal to 2`");
            return;
        }

        var indexBytes = new byte[4];
        RandomNumberGenerator.Fill(indexBytes);

        int index = BitConverter.ToInt32(indexBytes, 0) & int.MaxValue;
        index %= options.Count;

        await context.RespondAsync($"`you rolled and landed on {options[index]}`");
    }

    [Command("iqtest")]
    [Aliases("iq")]
    [Description("Determines IQ (is very real)")]
    [Cooldown(2, 5, CooldownBucketType.User)]
    [RequireGuild]
    public async Task DetermineIQAsync(CommandContext context, DiscordMember? member = null)
    {
        member ??= context.Member;

        Dictionary<int, string[]> phrases = new()
        {
            [70] = [
                "Your brain isn't underperforming - it's on strike.",
                "You took the IQ test and managed to offend the concept of intelligence itself.",
                "If thinking were a sport, you'd be benched for safety reasons."
            ],

            [85] = [
                "You're not technically stupid, just consistently underwhelming.",
                "If common sense were a superpower, you'd still be a civilian.",
                "You have just enough intelligence to realize you're not quite keeping up."
            ],

            [100] = [
                "Congratulations. You're perfectly average - in the most uninspiring way possible.",
                "You're the human version of 'meh.'",
                "If mediocrity had a mascot, you'd be on the cereal box."
            ],

            [115] = [
                "Smart enough to argue, not quite smart enough to win.",
                "You're the kind of intelligent that makes you dangerous... mostly to yourself.",
                "Did Elena Siegman take this test? Because the zombies are coming for you."
            ],

            [130] = [
                "You're sharp — just not sharp enough to cut through your own ego.",
                "You know a lot of things. It's just unfortunate that 'when to shut up' isn't one of them.",
                "You're impressive, in the way a calculator is impressive at a poetry slam."
            ],

            [150] = [
                "You're smart. Almost likable, if you weren't constantly reminding everyone of it.",
                "You make genius look exhausting.",
                "You're the kind of intelligent that gets bored with reality — and makes everyone else suffer for it.",
                "Congratulations, your brain is running a race no one else signed up for.",
                "You're so smart, you've looped back around to being socially awkward.",
                "With an IQ like that, you must find humanity incredibly disappointing. Welcome to the club.",
                "Congrats. You're the Stephen Hawking to god's Jeffery Epstein. A disabled, insufferable midget-toucher."
            ]
        };

        var iq = RandomNumberGenerator.GetInt32(70, 190);

        int matchedKey = phrases.Keys
            .Where(k => k <= iq)
            .Max();

        var selectedPhrases = phrases[matchedKey];
        var randomPhrase = selectedPhrases[RandomNumberGenerator.GetInt32(selectedPhrases.Length)];

        var embedBuilder = new DiscordEmbedBuilder()
        {
            Title = "House's IQ assessment",
            Description = $"Your IQ is `{iq}`. {randomPhrase}",
            Color = EmbedUtils.EmbedColor
        };

        embedBuilder.WithImageUrl(EmbedUtils.ImageURL);

        await context.RespondAsync(embedBuilder);
    }

    [Command("snipe")]
    [Aliases("getdeletedmessage")]
    [Description("Shows the last deleted message in this channel")]
    public async Task SnipeAsync(CommandContext context)
    {
        SnipeRepository repository = context.CommandsNext.Services.GetRequiredService<SnipeRepository>();

        SnipedMessage? sniped = await repository.GetLastMessageAsync(context.Channel.Id);

        if (sniped == null)
        {
            await context.RespondAsync("nothing to snipe!");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(sniped.AuthorName)
            .WithDescription(sniped.Content)
            .WithFooter($"Deleted at {sniped.DeletedAt:u}")
            .WithColor(DiscordColor.Orange);

        await context.RespondAsync(embed: embed.Build());
    }
}