using System.Text.Json;
using System.Text.Json.Serialization;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Net.Serialization;
using House.House.Attributes;
using House.House.Converters;
using House.House.Extensions;
using House.House.Services.Database;
using House.House.Services.Economy;
using House.House.Services.Fuzzy;
using House.House.Services.Protection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace House.House.Core;

[Obsolete("In favor of the new event registering system")]
public static class BotEventHandlers
{
    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new SnowflakeJSONConverter(), new AuditLogActionTypeConverter() },
    };

    [DiscordClientEvent("Ready")]
    public static Task OnReadyAsync(DiscordClient client, ReadyEventArgs args)
    {
        Console.WriteLine($"{client.CurrentUser.Username} is ready");

        DiscordActivity activity = new()
        {
            ActivityType = ActivityType.Watching,
            Name = "House's server"
        };

        client.UpdateStatusAsync(activity, UserStatus.Idle);

        return Task.CompletedTask;
    }

    /*
    [DiscordClientEvent("ChannelCreated")]
    public static async Task OnChannelCreatedAsync(DiscordClient client, ChannelCreateEventArgs args)
    {
        var commandsNext = client.GetCommandsNext();
        var antiNuke = commandsNext.Services.GetRequiredService<AntiNukeService>();

        var guild = args.Guild;
        var member = await GetResponsibleMemberAsync(guild, AuditLogActionType.ChannelCreate);

        if (member != null)
        {
            await antiNuke.HandleAuditLogEntryAsync("channel_create", guild, member);
        }
    }

    [DiscordClientEvent("ChannelDeleted")]
    public static async Task OnChannelDeletedAsync(DiscordClient client, ChannelDeleteEventArgs args)
    {
        var commandsNext = client.GetCommandsNext();
        var antiNuke = commandsNext.Services.GetRequiredService<AntiNukeService>();

        var guild = args.Guild;
        var member = await GetResponsibleMemberAsync(guild, AuditLogActionType.ChannelDelete);

        if (member != null)
        {
            await antiNuke.HandleAuditLogEntryAsync("channel_delete", guild, member);
        }
    }

    [DiscordClientEvent("GuildRoleCreated")]
    public static async Task OnRoleCreatedAsync(DiscordClient client, GuildRoleCreateEventArgs args)
    {
        var commandsNext = client.GetCommandsNext();
        var antiNuke = commandsNext.Services.GetRequiredService<AntiNukeService>();

        var guild = args.Guild;
        var member = await GetResponsibleMemberAsync(guild, AuditLogActionType.RoleCreate);

        if (member != null)
        {
            await antiNuke.HandleAuditLogEntryAsync("role_create", guild, member);
        }
    }

    [DiscordClientEvent("GuildRoleDeleted")]
    public static async Task OnRoleDeletedAsync(DiscordClient client, GuildRoleDeleteEventArgs args)
    {
        var commandsNext = client.GetCommandsNext();
        var antiNuke = commandsNext.Services.GetRequiredService<AntiNukeService>();

        var guild = args.Guild;
        var member = await GetResponsibleMemberAsync(guild, AuditLogActionType.RoleDelete);

        if (member != null)
        {
            await antiNuke.HandleAuditLogEntryAsync("role_delete", guild, member);
        }
    }

    [DiscordClientEvent("GuildBanAdded")]
    public static async Task OnGuildBanAddedAsync(DiscordClient client, GuildBanAddEventArgs args)
    {
        var commandsNext = client.GetCommandsNext();
        var antiNuke = commandsNext.Services.GetRequiredService<AntiNukeService>();

        var guild = args.Guild;
        var member = await GetResponsibleMemberAsync(guild, AuditLogActionType.Ban);

        if (member != null)
        {
            await antiNuke.HandleAuditLogEntryAsync("ban", guild, member);
        }
    }

    [DiscordClientEvent("GuildBanRemoved")]
    public static async Task OnGuildBanRemovedAsync(DiscordClient client, GuildBanRemoveEventArgs args)
    {
        var commandsNext = client.GetCommandsNext();
        var antiNuke = commandsNext.Services.GetRequiredService<AntiNukeService>();

        var guild = args.Guild;
        var member = await GetResponsibleMemberAsync(guild, AuditLogActionType.Ban);

        if (member != null)
        {
            await antiNuke.HandleAuditLogEntryAsync("unban", guild, member);
        }
    }

    private static async Task<DiscordMember?> GetResponsibleMemberAsync(DiscordGuild guild, AuditLogActionType type)
    {
        try
        {
            var logs = await guild.GetAuditLogsAsync(1, action_type: type);
            var entry = logs[0];

            return entry?.UserResponsible as DiscordMember;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get responsible user: {ex.Message}");
            return null;
        }
    }

    [DiscordClientEvent("GuildCreated")]
    public static async Task OnGuildCreatedAsync(DiscordClient client, GuildCreateEventArgs args)
    {
        var guild = args.Guild;

        var commandsNext = client.GetCommandsNext();
        var guildRepository = commandsNext.Services.GetRequiredService<GuildRepository>();

        var foundGuild = await guildRepository.TryGetAsync(guild.Id.ToString());
        if (foundGuild != null)
        {
            if (foundGuild.DefaultChannelID != null)
            {
                var channel = guild.GetChannel(foundGuild.DefaultChannelID.Value);
                await channel.SendMessageAsync("House bot in the house!");
            }
            else
            {
                var channel = guild.GetDefaultChannel();
                await channel.SendMessageAsync("House bot in the house!");
            }

            return;
        }

        DatabaseGuild databaseGuild = new()
        {
            ID = guild.Id.ToString(),
            Name = guild.Name,
        };

        await guildRepository.AddAsync(databaseGuild);
    }

    [DiscordClientEvent("GuildMemberAdded")]
    public static async Task OnGuildMemberAddedAsync(DiscordClient client, GuildMemberAddEventArgs args)
    {
        var guild = args.Guild;
        var member = args.Member;

        var commandsNext = client.GetCommandsNext();

        var blacklistedUserRepository = commandsNext.Services.GetRequiredService<BlacklistedUserRepository>();

        if (member.IsBot)
        {
            bool isVerified = member.Flags?.HasFlag(UserFlags.VerifiedBot) ?? member.Verified.GetValueOrDefault(false);

            if (!isVerified)
            {
                await member.BanAsync();

                var defaultChannel = guild.GetDefaultChannel();
                await defaultChannel.SendMessageAsync($"{member.Mention} was banned due to being a unverified bot");
            }
        }

        var blacklistedUser = await blacklistedUserRepository.TryGetAsync(member.Id.ToString());
        if (blacklistedUser != null)
        {
            await member.RemoveAsync("Blacklisted user");

            var defaultChannel = guild.GetDefaultChannel();
            await defaultChannel.SendMessageAsync($"{member.Mention} was banned because they are blacklisted");

            return;
        }
    }
    */

    [DiscordClientEvent("UnknownEvent")]
    public static Task OnUnknownEvent(DiscordClient client, UnknownEventArgs args)
    {
        string eventName = args.EventName;
        client.Logger.LogInformation("Unknown event received: {EventName}", eventName);

        return Task.CompletedTask;
    }

    [DiscordClientEvent("MessageReactionAdded")]
    public static async Task OnMessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
    {
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

    [DiscordClientEvent("MessageCreated")]
    public static async Task OnMessageCreatedAsync(DiscordClient client, MessageCreateEventArgs args)
    {
        var author = args.Author;
        if (author.IsBot)
        {
            return;
        }

        var message = args.Message;

        // await antiNuke.HandleMessageAsync(message);

        if (message.MentionedUsers.Any(u => u == client.CurrentUser))
        {
            await message.Channel.SendMessageAsync("Yes, hello!");
            return;
        }

        var commandsNext = client.GetCommandsNext();

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
            var results = fuzzyService.GetResults(commandName, 50, mode: ONFQ.ONFQ.Models.BlendMode.BlendAll);
            var embed = await fuzzyService.ToDiscordEmbed(commandName, results);

            await message.Channel.SendMessageAsync(embed);

            return;
        }

        var context = commandsNext.CreateContext(message, matchedPrefix, command, rawArguments);
        await commandsNext.ExecuteCommandAsync(context);
    }

    [CommandsNextEvent("CommandErrored")]
    public static async Task OnCommandErroredAsync(CommandsNextExtension _, CommandErrorEventArgs args)
    {
        var context = args.Context;
        var exception = args.Exception;

        switch (exception)
        {
            case ChecksFailedException checks:
                await HandleFailedChecksAsync(context, checks);
                break;

            case CommandNotFoundException:
                await context.RespondAsync("`that command does not exist`");
                break;

            case ArgumentException or ArgumentNullException:
                await context.RespondAsync(exception.Message);
                break;

            /*
            case CoomerHTTPException httpEx:
                await context.RespondAsync($"`http {(int)httpEx.StatusCode} error`");
                break;
            */

            case UserNotFoundException notFoundException:
                await context.RespondAsync($"`{notFoundException.Message}`");
                break;

            case NoBalanceChangeProvidedException noBalanceChangeProvided:
                await context.RespondAsync($"`{noBalanceChangeProvided.Message}`");
                break;

            case UserAlreadyExistsException alreadyExistsException:
                await context.RespondAsync($"`{alreadyExistsException.Message}`");
                break;

            case EntityExistsException entityExistsException:
                await context.RespondAsync($"`{entityExistsException.Message}`");
                break;

            case EntityNotFoundException notFoundException:
                await context.RespondAsync($"`{notFoundException.Message} 1`");
                break;

            /*
            case CoomerCreatorNotFoundException creatorEx:
                await context.RespondAsync($"`creator '{creatorEx.Service}/{creatorEx.Username}' was not found`");
                break;

            case CoomerPostNotFoundException postEx:
                await context.RespondAsync($"`post with id '{postEx.PostId}' was not found`");
                break;

            case CoomerDeserializationException desEx:
                Console.WriteLine(desEx);

                await context.RespondAsync("`failed to parse data from coomer`");
                break;

            case CoomerClientException clientEx:
                Console.WriteLine(clientEx);

                await context.RespondAsync("`something went wrong in the coomer client`");
                break;

            case CoomerPostsNotFoundException postsEx:
                await context.RespondAsync($"`no posts found for {postsEx.Service}/{postsEx.Username}`");
                break;

            case CoomerServiceException serviceEx:
                Console.WriteLine(serviceEx);

                await context.RespondAsync("`a coomer service error occurred`");
                break;
            */

            case NotFoundException:
                break;

            default:
                Console.WriteLine($"[ERROR] {exception.Message}\n{exception.StackTrace}");

                break;
        }
    }

    private static async Task HandleFailedChecksAsync(CommandContext context, ChecksFailedException exception)
    {
        foreach (var check in exception.FailedChecks)
        {
            var message = check switch
            {
                IsOwnerAttribute => "`this command is restricted to only the owner, which you are not`",
                CooldownAttribute cooldown => $"`you can use this command again in {cooldown.GetRemainingCooldown(context).TotalSeconds:F1} seconds`",
                RequireUserPermissionsAttribute requireUserPermissions => $"`you are missing permissions {string.Join(", ", requireUserPermissions.Permissions)}`",
                RequireBotPermissionsAttribute requireBotPermissions => $"`i am missing permissions {string.Join(", ", requireBotPermissions.Permissions)}`",
                IsPlayerAttribute => "`you are not registered as a player in the database`",
                IsStaffAttribute => "`you are not a staff member with sufficient privileges to use this command`",
                IsStaffOrOwnerAttribute => "`you must be staff or a bot owner to use this command`",
                RequireDirectMessageAttribute => "`you must be in direct messages to use this command`",
                _ => $"`requirement check failed. requirement is: {check}`"
            };

            await context.RespondAsync(message);
            return;
        }
    }

    private static (int, string?) FindMatchingPrefixes(Config config, DiscordMessage message)
    {
        foreach (var prefix in config.DefaultPrefixes)
        {
            int index = message.GetStringPrefixLength(prefix);
            if (index == -1)
            {
                continue;
            }

            return (index, prefix);
        }

        return (-1, null);
    }
}

/*
public static class BotEventHandlers
{
    public static async Task OnReadyAsync(HouseBotEvent evt)
    {
        if (evt.Name != "Ready")
        {
            return;
        }

        var client = evt.Sender;

        Console.WriteLine($"{client.CurrentUser.Username} is ready");

        var activity = new DiscordActivity
        {
            ActivityType = ActivityType.Watching,
            Name = "House's server"
        };

        await client.UpdateStatusAsync(activity, UserStatus.Idle);
    }

    public static async Task OnMessageCreated(HouseBotEvent evt)
    {
        if (evt.Name != "MessageCreated")
        {
            return;
        }

        var args = (MessageCreateEventArgs)evt.EventArgs;
        var message = args.Message;
        var author = args.Author;
        var client = evt.Sender;

        if (author.IsBot) return;

        var commandsNext = client.GetCommandsNext();

        var fuzzyService = commandsNext.Services.GetRequiredService<HouseFuzzyMatchingService>();
        var config = commandsNext.Services.GetRequiredService<Config>();

        var (pos, matchedPrefix) = FindMatchingPrefixes(config, message);
        if (pos == -1 || matchedPrefix == null)
        {
            return;
        }

        var commandString = message.Content[pos..].Trim();
        if (string.IsNullOrWhiteSpace(commandString))
        {
            return;
        }

        var parts = commandString.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var commandName = parts.Length > 0 ? parts[0] : string.Empty;
        var rawArgs = parts.Length > 1 ? parts[1] : string.Empty;

        var command = commandsNext.FindCommand(commandName, out _);
        if (command == null)
        {
            var results = fuzzyService.GetResults(commandName, 50, mode: ONFQ.ONFQ.Models.BlendMode.BlendAll);
            var embed = await fuzzyService.ToDiscordEmbed(commandName, results);

            await message.Channel.SendMessageAsync(embed);

            return;
        }

        var context = commandsNext.CreateContext(message, matchedPrefix, command, rawArgs);
        await commandsNext.ExecuteCommandAsync(context);
    }

    public static async Task OnMessageReactionAdded(HouseBotEvent evt)
    {
        if (evt.Name != "MessageReactionAdded")
        {
            return;
        }

        var args = (MessageReactionAddEventArgs)evt.EventArgs;
        var guild = args.Guild;
        var message = args.Message;
        var client = evt.Sender;

        var commandsNext = client.GetCommandsNext();
        var guildRepository = commandsNext.Services.GetRequiredService<GuildRepository>();

        var found = await guildRepository.TryGetAsync(guild.Id);

        if (found == null || !found.StarboardChannelID.HasValue)
        {
            return;
        }

        var starboardChannel = guild.GetChannel(found.StarboardChannelID.Value) ?? guild.SystemChannel;
        if (starboardChannel == null)
        {
            return;
        }

        var starEmoji = DiscordEmoji.FromUnicode("⭐");
        var starReaction = message.Reactions.FirstOrDefault(r => r.Emoji == starEmoji);

        if (starReaction != null && starReaction.Count >= 2)
        {
            var embedBuilder = new DiscordEmbedBuilder
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
                embedBuilder.WithImageUrl(message.Attachments[0].Url);
            }

            await starboardChannel.SendMessageAsync(embedBuilder);
        }
    }

    public static async Task OnCommandErrored(HouseCommandsNextEvent evt)
    {
        if (evt.Name != "CommandErrored")
        {
            return;
        }

        var args = (CommandErrorEventArgs)evt.EventArgs;
        var context = args.Context;
        var exception = args.Exception;

        switch (exception)
        {
            case ChecksFailedException checks:
                await HandleFailedChecksAsync(context, checks);
                break;

            case CommandNotFoundException:
                await context.RespondAsync("`that command does not exist`");
                break;

            case ArgumentException or ArgumentNullException:
                await context.RespondAsync(exception.Message);
                break;

            case UserNotFoundException notFound:
                await context.RespondAsync($"`{notFound.Message}`");
                break;

            case NoBalanceChangeProvidedException noBalance:
                await context.RespondAsync($"`{noBalance.Message}`");
                break;

            case UserAlreadyExistsException alreadyExists:
                await context.RespondAsync($"`{alreadyExists.Message}`");
                break;

            case EntityExistsException entityExists:
                await context.RespondAsync($"`{entityExists.Message}`");
                break;

            case EntityNotFoundException notFound2:
                await context.RespondAsync($"`{notFound2.Message} 1`");
                break;

            default:
                Console.WriteLine($"[ERROR] {exception.Message}\n{exception.StackTrace}");
                break;
        }
    }

    private static async Task HandleFailedChecksAsync(CommandContext context, ChecksFailedException exception)
    {
        foreach (var check in exception.FailedChecks)
        {
            var message = check switch
            {
                IsOwnerAttribute => "`this command is restricted to only the owner, which you are not`",
                CooldownAttribute cooldown => $"`you can use this command again in {cooldown.GetRemainingCooldown(context).TotalSeconds:F1} seconds`",
                RequireUserPermissionsAttribute requireUserPermissions => $"`you are missing permissions {string.Join(", ", requireUserPermissions.Permissions)}`",
                RequireBotPermissionsAttribute requireBotPermissions => $"`i am missing permissions {string.Join(", ", requireBotPermissions.Permissions)}`",
                IsPlayerAttribute => "`you are not registered as a player in the database`",
                IsStaffAttribute => "`you are not a staff member with sufficient privileges to use this command`",
                IsStaffOrOwnerAttribute => "`you must be staff or a bot owner to use this command`",
                RequireDirectMessageAttribute => "`you must be in direct messages to use this command`",
                _ => $"`requirement check failed. requirement is: {check}`"
            };

            await context.RespondAsync(message);
            return;
        }
    }

    private static (int, string?) FindMatchingPrefixes(Config config, DiscordMessage message)
    {
        foreach (var prefix in config.DefaultPrefixes)
        {
            int index = message.GetStringPrefixLength(prefix);
            if (index == -1)
            {
                continue;
            }

            return (index, prefix);
        }

        return (-1, null);
    }
}*/