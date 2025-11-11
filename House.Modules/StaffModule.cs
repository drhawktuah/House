using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using House.House.Attributes;
using House.House.Core;
using House.House.Services.Database;
using House.House.Services.Protection;
using Microsoft.Extensions.DependencyInjection;

namespace House.House.Modules;

[Description("House's staff module")]
public sealed class StaffModule : BaseCommandModule
{
    public required Config Config { get; set; }

    [Command("addrole")]
    [Aliases("giverole", "giveuserrole", "adduserrole", "ar")]
    [Cooldown(1, 5, CooldownBucketType.User)]
    [RequirePermissions(Permissions.BanMembers | Permissions.KickMembers | Permissions.ManageRoles)]
    [RequireBotPermissions(Permissions.Administrator)]
    [RequireGuild]
    public async Task AddRoleAsync(CommandContext context, DiscordMember member, DiscordRole role)
    {
        var guild = context.Guild;

        var botRole = guild.CurrentMember.Roles.First();

        if (role.Position > botRole.Position)
        {
            await context.RespondAsync($"I cannot add {role} to {member} because it is higher than mine");
            return;
        }

        var authorRole = context.Member?.Roles.First();

        if (role.Position > authorRole?.Position)
        {
            await context.RespondAsync($"You cannot add {role} to {member} because it is higher than yours");
            return;
        }

        if (member.Roles.Contains(role))
        {
            await context.RespondAsync($"{member} already has {role}");
            return;
        }

        await member.GrantRoleAsync(role);
        await context.RespondAsync($"{role} has been successfully granted to {member}");
    }

    [Command("createrole")]
    [Aliases("newrole")]
    [Cooldown(2, 10, CooldownBucketType.Guild)]
    [RequirePermissions(Permissions.BanMembers | Permissions.KickMembers | Permissions.ManageRoles)]
    [RequireBotPermissions(Permissions.Administrator)]
    [RequireGuild]
    public async Task CreateRoleAsync(CommandContext context, params string[] data)
    {
        if (data.Length < 1)
        {
            await context.RespondAsync("`createrole <name> [color] [mentionable:yes|no]`");
            return;
        }

        string name = data[0];

        if (string.IsNullOrWhiteSpace(name))
        {
            await context.RespondAsync("`you must specify a name`");
            return;
        }

        string? colorInput = data.Length > 1 ? data[1] : null;
        string? mentionableInput = data.Length > 2 ? data[2].ToLower() : null;

        bool mentionable = mentionableInput switch
        {
            "yes" or "y" or "true" => true,
            "no" or "n" or "false" => false,
            _ => false,
        };

        DiscordColor color = DiscordColor.None;

        if (!string.IsNullOrWhiteSpace(colorInput))
        {
            if (colorInput.StartsWith('#'))
            {
                colorInput = colorInput[1..];
            }

            if (int.TryParse(colorInput, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hex))
            {
                color = new(hex);
            }
            else
            {
                color = colorInput.ToLowerInvariant() switch
                {
                    "red" => DiscordColor.Red,
                    "green" => DiscordColor.Green,
                    "blue" => DiscordColor.Blue,
                    "yellow" => DiscordColor.Yellow,
                    "purple" => DiscordColor.Purple,
                    "orange" => DiscordColor.Orange,
                    "black" => DiscordColor.Black,
                    "white" => DiscordColor.White,
                    "cyan" => DiscordColor.Cyan,
                    "pink" => DiscordColor.HotPink,
                    _ => DiscordColor.None
                };
            }
        }

        DiscordRole role = await context.Guild.CreateRoleAsync(name, permissions: Permissions.None, color, hoist: false, mentionable);

        await context.RespondAsync($"created `{role.Name}`");
    }

    [Command("removerole")]
    [Aliases("rr")]
    [Cooldown(1, 5, CooldownBucketType.User)]
    [RequirePermissions(Permissions.BanMembers | Permissions.KickMembers | Permissions.ManageRoles)]
    [RequireBotPermissions(Permissions.Administrator)]
    [RequireGuild]
    public async Task RemoveRoleAsync(CommandContext context, DiscordMember member, DiscordRole role)
    {
        var guild = context.Guild;
        var botRole = guild.CurrentMember.Roles.First();

        if (context.Member == member)
        {
            await context.RespondAsync($"you cannot give yourself {role}");
            return;
        }

        if (role.Position > botRole.Position)
        {
            await context.RespondAsync($"I cannot remove {role} from {member} because it is higher than mine");
            return;
        }

        var authorRole = context.Member?.Roles.First();

        if (role.Position > authorRole?.Position)
        {
            await context.RespondAsync($"You cannot remove {role} from {member} because it is higher than yours");
            return;
        }

        if (!member.Roles.Contains(role))
        {
            await context.RespondAsync($"{member} does not have {role}");
            return;
        }

        await member.RevokeRoleAsync(role);
        await context.RespondAsync($"{role} has been successfully removed from {member}");
    }

    [Aliases("kickuser", "kickmember")]
    [Command("kick")]
    [Description("Kicks a user within the server")]
    [RequireGuild]
    [RequireBotPermissions(Permissions.KickMembers)]
    [RequireUserPermissions(Permissions.KickMembers)]
    public async Task KickMemberAsync(CommandContext context, DiscordMember member, [RemainingText] string reason = "none provided")
    {
        if (member == context.Member)
        {
            await context.RespondAsync($"you cannot kick yourself");
            return;
        }

        if (member.Hierarchy > context.Member?.Hierarchy)
        {
            await context.RespondAsync($"you cannot kick `{member.Username}` due to their position being higher than yours");
            return;
        }

        if (member.Hierarchy > context.Guild.CurrentMember.Hierarchy)
        {
            await context.RespondAsync($"i cannot kick `{member.Username}` due to their position being higher than mine");
            return;
        }

        var memberRole = member.Roles.FirstOrDefault();

        if (memberRole is not null && (memberRole.Permissions.HasPermission(Permissions.BanMembers) | memberRole.Permissions.HasPermission(Permissions.KickMembers)))
        {
            await context.RespondAsync($"`{member.Username} has moderator permissions, are you sure you want to kick them?`");

            var interactivity = context.Client.GetInteractivity();
            var waited = await interactivity.WaitForMessageAsync(x => x.Channel == context.Channel && x.Author == context.User, TimeSpan.FromSeconds(15));

            if (waited.TimedOut)
            {
                await context.RespondAsync($"`timed out whilst authenticating request to kick {member.Username}`");
                return;
            }

            var result = waited.Result.Content.Trim().ToLowerInvariant();

            if (result is not ("yes" or "y"))
            {
                await context.RespondAsync($"`request to kick {member.Username} has been cancelled`");
                return;
            }
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Kicked {member.Username}")
            .WithThumbnail(member.AvatarUrl);

        await member.RemoveAsync(reason);

        await context.Channel.SendMessageAsync(embed);
    }

    [Command("ban")]
    [RequireGuild]
    [Aliases("banuser", "banmember")]
    [Description("Bans a user either outside of the server or within the server")]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    public async Task BanMemberAsync(CommandContext context, DiscordMember member, int deletedDays = 0, [RemainingText] string reason = "none provided")
    {
        if (member == context.Member)
        {
            await context.RespondAsync($"you cannot ban yourself");
            return;
        }

        if (member.Hierarchy > context.Member?.Hierarchy)
        {
            await context.RespondAsync($"you cannot ban `{member.Username}` due to their position being higher than yours");
            return;
        }

        if (member.Hierarchy > context.Guild.CurrentMember.Hierarchy)
        {
            await context.RespondAsync($"i cannot ban `{member.Username}` due to their position being higher than mine");
            return;
        }

        var memberRole = member.Roles.FirstOrDefault();
        if (memberRole is not null && (memberRole.Permissions.HasPermission(Permissions.BanMembers) | memberRole.Permissions.HasPermission(Permissions.KickMembers)))
        {
            await context.RespondAsync($"`{member.Username} has moderator permissions, are you sure you want to ban them?`");

            var interactivity = context.Client.GetInteractivity();
            var waited = await interactivity.WaitForMessageAsync(x => x.Channel == context.Channel && x.Author == context.User, TimeSpan.FromSeconds(15));

            if (waited.TimedOut)
            {
                await context.RespondAsync($"`timed out whilst authenticating request to ban {member.Username}`");
                return;
            }

            var result = waited.Result.Content.Trim().ToLowerInvariant();

            if (result is not ("yes" or "y"))
            {
                await context.RespondAsync($"`request to ban {member.Username} has been cancelled`");
                return;
            }
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Banned {member.Username}")
            .WithThumbnail(member.AvatarUrl);

        await member.BanAsync(deletedDays, reason);

        await context.Channel.SendMessageAsync(embed);
    }

    public async Task BanMemberAsync(CommandContext context, ulong memberID, [RemainingText] string reason = "none provided")
    {
        var member = await context.Client.GetUserAsync(memberID);

        if (member == context.Member)
        {
            await context.RespondAsync($"you cannot ban yourself");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Banned {member.Username}")
            .WithThumbnail(member.AvatarUrl);

        await context.Guild.BanMemberAsync(memberID, reason: reason);

        await context.Channel.SendMessageAsync(embed);
    }

    [Aliases("purgemessages", "deletemessages")]
    [Command("purge")]
    [Description("Purges messages within a channel")]
    [RequireGuild]
    [RequireBotPermissions(Permissions.ManageMessages)]
    [RequireUserPermissions(Permissions.ManageChannels)]
    public async Task PurgeMessagesAsync(CommandContext context, int amount = 10, [RemainingText] string? filter = null)
    {
        if (!string.IsNullOrWhiteSpace(filter))
        {
            filter = Regex.Escape(filter.ToLowerInvariant());
        }

        var messages = await context.Channel.GetMessagesAsync(amount);

        if (!string.IsNullOrWhiteSpace(filter))
        {
            messages = [.. messages.Where(m => !m.Author.IsBot && Regex.IsMatch(m.Content.ToLowerInvariant(), filter))];
        }
        else
        {
            messages = messages
                .Where(m => !m.Author.IsBot)
                .ToList();
        }

        await context.Channel.DeleteMessagesAsync(messages);

        if (filter is not null)
        {
            await context.Channel.SendMessageAsync($"purged `{amount}` messages with {filter}");
        }
        else
        {
            await context.Channel.SendMessageAsync($"purged `{amount}` messages");
        }
    }

    [Command("unban")]
    [RequireGuild]
    [Aliases("unbanuser", "unbanmember")]
    [Description("Unbans a user")]
    [RequireUserPermissions(Permissions.BanMembers)]
    [RequireBotPermissions(Permissions.BanMembers)]
    public async Task UnbanMemberAsync(CommandContext context, DiscordUser user, string reason = "none provided")
    {
        if (user == context.User)
        {
            await context.RespondAsync($"you cannot unban yourself");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Unbanned {user.Username}")
            .WithThumbnail(user.AvatarUrl);

        await context.Guild.UnbanMemberAsync(user, reason);
        await context.Channel.SendMessageAsync(embed);
    }

    [Command("lock")]
    [RequireGuild]
    [Aliases("lockchannel", "lockdown")]
    [Description("Locks a channel")]
    [RequireUserPermissions(Permissions.ManageChannels)]
    [RequireBotPermissions(Permissions.ManageChannels)]
    public async Task LockChannelAsync(CommandContext context)
    {
        var channel = context.Channel;

        await channel.AddOverwriteAsync(context.Guild.EveryoneRole, Permissions.None, Permissions.SendMessages, "lockdown");
        await channel.SendMessageAsync($"locked channel `{channel.Name}`");
    }

    [Command("unlock")]
    [RequireGuild]
    [Aliases("unlockchannel", "removelockdown")]
    [Description("Unlocks a channel")]
    [RequireUserPermissions(Permissions.ManageChannels)]
    [RequireBotPermissions(Permissions.ManageChannels)]
    public async Task UnlockChannelAsync(CommandContext context)
    {
        var channel = context.Channel;

        await channel.AddOverwriteAsync(context.Guild.EveryoneRole, Permissions.SendMessages, Permissions.None, "lockdown");
        await channel.SendMessageAsync($"unlocked channel `{channel.Name}`");
    }

    /*
    [Command("punishedusers")]
    [RequireGuild]
    [Aliases("getpunishedusers")]
    [Description("Gets members who tried to raid and got fucked, lol")]
    [RequireUserPermissions(Permissions.KickMembers)]
    [RequireBotPermissions(Permissions.KickMembers)]
    public async Task GetPunishedUsersAsync(CommandContext context)
    {
        var antiNukeService = context.CommandsNext.Services.GetRequiredService<AntiNukeService>();

        var users = await antiNukeService.GetPunishedUsersAsync(context.Guild);
        if (users.Count == 0)
        {
            await context.RespondAsync("No members have been flagged");
            return;
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = "Flagged members",
            Description = $"```xl\n{string.Join('\n', users)}\n```"
        };

        await context.RespondAsync(embedBuilder);
    }
    */

    [Command("addguild")]
    [RequireGuild]
    [Aliases("createguildconfig")]
    [Description("Creates a recognizable guild config for the bot")]
    [IsGuildOwner]
    [IsConfigExisting]
    public async Task CreateGuildConfigAsync(CommandContext context)
    {
        var interactivity = context.Client.GetInteractivity();
        var guild = context.Guild;

        var newConfig = new DatabaseGuild
        {
            ID = context.Guild.Id
        };

        await context.RespondAsync("Please enter the name for the guild configuration:");
        var nameResponse = await interactivity.WaitForMessageAsync(
            x => x.Author.Id == context.User.Id && x.ChannelId == context.Channel.Id,
            TimeSpan.FromMinutes(2));

        if (nameResponse.TimedOut)
        {
            await context.RespondAsync("Timed out. Please try the command again");
            return;
        }

        newConfig.Name = nameResponse.Result.Content;

        await context.RespondAsync("Please mention the default channel for logs, or provide its ID:");

        var channelResponse = await interactivity.WaitForMessageAsync(
            x => x.Author.Id == context.User.Id && x.ChannelId == context.Channel.Id,
            TimeSpan.FromMinutes(2));

        if (channelResponse.TimedOut)
        {
            await context.RespondAsync("Timed out. Please try the command again");
            return;
        }

        var channelMsg = channelResponse.Result;

        ulong defaultChannelId = 0;

        if (channelMsg.MentionedChannels.Count > 0)
        {
            defaultChannelId = channelMsg.MentionedChannels[0].Id;
        }
        else if (ulong.TryParse(channelMsg.Content, out var parsedId))
        {
            var ch = guild.GetChannel(parsedId);
            if (ch == null)
            {
                await context.RespondAsync("Channel not found in this guild. Command cancelled");
                return;
            }
            defaultChannelId = parsedId;
        }
        else
        {
            await context.RespondAsync("Invalid channel input. Command cancelled");
            return;
        }

        newConfig.DefaultChannelID = defaultChannelId;

        await context.RespondAsync("Please mention the starboard channel, or provide its ID:");

        var starboardResponse = await interactivity.WaitForMessageAsync(
            x => x.Author.Id == context.User.Id && x.ChannelId == context.Channel.Id,
            TimeSpan.FromMinutes(2));

        if (starboardResponse.TimedOut)
        {
            await context.RespondAsync("Timed out. Please try the command again");
            return;
        }

        var starboardMessage = starboardResponse.Result;

        ulong starboardChannelID = 0;

        if (channelMsg.MentionedChannels.Count > 0)
        {
            starboardChannelID = starboardMessage.MentionedChannels[0].Id;
        }
        else if (ulong.TryParse(starboardMessage.Content, out var parsedId))
        {
            var ch = guild.GetChannel(parsedId);
            if (ch == null)
            {
                await context.RespondAsync("Channel not found in this guild. Command cancelled");
                return;
            }

            starboardChannelID = parsedId;
        }
        else
        {
            await context.RespondAsync("Invalid channel input. Command cancelled");
            return;
        }

        newConfig.StarboardChannelID = starboardChannelID;

        await context.RespondAsync("Please mention the punishment role, provide its ID, or type `create` to create a new role:");

        var roleResponse = await interactivity.WaitForMessageAsync(
            x => x.Author.Id == context.User.Id && x.ChannelId == context.Channel.Id,
            TimeSpan.FromMinutes(2));

        if (roleResponse.TimedOut)
        {
            await context.RespondAsync("Timed out. Punishment role will be left empty");
            newConfig.PunishmentRole = null;
        }
        else
        {
            var roleMsg = roleResponse.Result;

            if (roleMsg.Content.Trim().Equals("create", StringComparison.InvariantCultureIgnoreCase))
            {
                var role = await guild.CreateRoleAsync("Punishment Role", Permissions.None, null, false, false, "Created by AntiNukeService");

                newConfig.PunishmentRole = role.Id;

                await context.RespondAsync($"Created new punishment role: {role.Name}");
            }
            else
            {
                ulong punishmentRoleId = 0;
                if (roleMsg.MentionedRoles.Count > 0)
                {
                    punishmentRoleId = roleMsg.MentionedRoles[0].Id;
                }
                else if (ulong.TryParse(roleMsg.Content, out var parsedRoleId))
                {
                    var role = guild.GetRole(parsedRoleId);
                    if (role == null)
                    {
                        try
                        {
                            var newRole = await guild.CreateRoleAsync(
                                name: "Punishment Role",
                                permissions: Permissions.None,
                                color: DiscordColor.Red,
                                hoist: false,
                                mentionable: false,
                                reason: "Creating punishment role due to invalid input");

                            punishmentRoleId = newRole.Id;
                            newConfig.PunishmentRole = punishmentRoleId;

                            await context.RespondAsync($"Role not found. Created new punishment role: {newRole.Name}");
                        }
                        catch (Exception ex)
                        {
                            await context.RespondAsync($"Failed to create punishment role: {ex.Message}");
                            newConfig.PunishmentRole = null;
                        }
                    }
                    else
                    {
                        punishmentRoleId = parsedRoleId;
                        newConfig.PunishmentRole = punishmentRoleId;
                    }
                }
                else
                {
                    try
                    {
                        var newRole = await guild.CreateRoleAsync(
                            name: "Punishment Role",
                            permissions: Permissions.None,
                            color: DiscordColor.Red,
                            hoist: false,
                            mentionable: false,
                            reason: "Creating punishment role due to invalid input");

                        punishmentRoleId = newRole.Id;
                        newConfig.PunishmentRole = punishmentRoleId;

                        await context.RespondAsync($"Invalid input. Created new punishment role: {newRole.Name}");
                    }
                    catch (Exception ex)
                    {
                        await context.RespondAsync($"Failed to create punishment role: {ex.Message}");
                        newConfig.PunishmentRole = null;
                    }
                }
            }
        }

        await context.RespondAsync("Please select protection level:\n" +
            "`0` - None\n" +
            "`1` - Basic\n" +
            "`2` - Minimal\n" +
            "`3` - SuperStrict\n" +
            "`4` - ExtremelyStrict\n" +
            "`5` - Members");

        var protectionResponse = await interactivity.WaitForMessageAsync(
            x => x.Author.Id == context.User.Id && x.ChannelId == context.Channel.Id,
            TimeSpan.FromMinutes(2));

        if (protectionResponse.TimedOut)
        {
            await context.RespondAsync("Timed out. Using default protection level (Basic)");
            newConfig.ProtectionLevel = ProtectionLevel.Basic;
        }
        else
        {
            var content = protectionResponse.Result.Content.Trim();

            if (int.TryParse(content, out int levelInt) && Enum.IsDefined(typeof(ProtectionLevel), levelInt))
            {
                newConfig.ProtectionLevel = (ProtectionLevel)levelInt;
            }
            else
            {
                await context.RespondAsync("Invalid input. Using default protection level (Basic)");
                newConfig.ProtectionLevel = ProtectionLevel.Basic;
            }
        }

        newConfig.JoinedAt = DateTime.UtcNow;

        var guildRepo = context.Services.GetService<GuildRepository>();

        if (guildRepo == null)
        {
            await context.RespondAsync("Guild repository not available, cannot save config");
            return;
        }

        await guildRepo.AddAsync(newConfig);

        await context.RespondAsync("Guild configuration created successfully!");
    }
}

/*
[Group("antinuke")]
[Description("anti-nuke services")]
public sealed class AntiNukeModule : BaseCommandModule
{
    public required AntiNukeService AntiNukeService { get; set; }

    [Command("removeuser")]
    [Aliases("removemember")]
    [IsStaffOrOwner(Position.Admin)]
    public async Task RemovePunishedUserAsync(CommandContext context, DiscordMember member)
    {
        PunishedMember? punishedMember = await AntiNukeService.GetPunishedMemberAsync(member);

        if (punishedMember == null)
        {
            await context.RespondAsync($"{member.Username} has not been punished");
            return;
        }

        StringBuilder builder = new()
        {
            Capacity = 512
        };

        builder.AppendLine("violations:");

        foreach (var (key, value) in punishedMember.Actions)
        {
            builder.AppendLine($"{value} - {key}");
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = member.Username,
            Description = builder.ToString()
        };

        await context.RespondAsync(embedBuilder);
    }
}
*/