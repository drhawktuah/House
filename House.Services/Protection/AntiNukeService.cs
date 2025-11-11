using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using House.House.Core;
using House.House.Extensions;
using House.House.Services.Database;
using House.House.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace House.House.Services.Protection;

public sealed class AntiNukeService
{
    public SuspectManager SuspectManager { get; }

    private readonly WhitelistedUserRepository whitelistedUserRepository;
    private readonly BlacklistedUserRepository blacklistedUserRepository;
    private readonly SuspectMemberRepository suspectMemberRepository;
    private readonly GuildRepository guildRepository;
    private readonly StaffUserRepository staffUserRepository;

    private readonly ConcurrentDictionary<ulong, List<DateTime>> messageTimestamps = [];
    private readonly ConcurrentDictionary<ulong, int> messageViolations = [];

    public AntiNukeService(DiscordClient client)
    {
        SuspectManager = new();

        var commandsNext = client.GetCommandsNext();
        var services = commandsNext.Services;

        whitelistedUserRepository = services.GetRequiredService<WhitelistedUserRepository>();
        blacklistedUserRepository = services.GetRequiredService<BlacklistedUserRepository>();
        suspectMemberRepository = services.GetRequiredService<SuspectMemberRepository>();
        guildRepository = services.GetRequiredService<GuildRepository>();
        staffUserRepository = services.GetRequiredService<StaffUserRepository>();

        client.Logger.LogInformation("Anti-Nuke Service initialized successfully.");
    }

    public async Task HandleMessageAsync(DiscordMessage message)
    {
        if (message.Author is not DiscordMember member)
        {
            return;
        }

        if (await IsStaffAsync(member) || await IsWhitelistedAsync(member))
        {
            return;
        }

        var guild = message.Channel.Guild;
        if (!await GuildExistsAsync(guild))
        {
            return;
        }

        if (!messageTimestamps.TryGetValue(member.Id, out var timestamps))
        {
            timestamps = [];
            messageTimestamps[member.Id] = timestamps;
        }

        DateTime now = DateTime.UtcNow;
        timestamps.Add(now);

        TimeSpan spamWindow = TimeSpan.FromSeconds(5);
        timestamps.RemoveAll(ts => ts + spamWindow < now);

        int spamThreshold = 5;
        if (timestamps.Count >= spamThreshold)
        {
            if (!messageViolations.TryGetValue(member.Id, out int violations))
            {
                violations = 0;
            }

            violations++;
            messageViolations[member.Id] = violations;

            var databaseGuild = await guildRepository.TryGetAsync(guild.Id);
            if (databaseGuild is not null)
            {
                switch (violations)
                {
                    case 1:
                        await ApplyPunishmentAsync(member, guild, databaseGuild, "first-level", DateTimeOffset.UtcNow.AddMinutes(1));
                        break;
                    case 2:
                        await ApplyPunishmentAsync(member, guild, databaseGuild, "second-level", DateTimeOffset.UtcNow.AddMinutes(30));
                        break;
                    default:
                        await member.RemoveAsync("third-level spam punishment");
                        messageViolations.Remove(member.Id, out _);
                        break;
                }
            }

            timestamps.Clear();
        }
    }

    public async Task HandleAuditLogActionAsync(DiscordAuditLogEntry entry, DiscordGuild guild)
    {
        if (entry.UserResponsible is not DiscordMember member)
        {
            return;
        }

        if (member.IsBot)
        {
            if (!member.Verified.HasValue || !member.Verified.Value)
            {
                await member.BanAsync(reason: "Unverified bot detected");
                return;
            }
            else
            {
                return;
            }
        }

        SuspectManager.AddOrUpdate(member);

        if (await IsStaffAsync(member) || await IsWhitelistedAsync(member))
        {
            return;
        }

        if (!await GuildExistsAsync(guild))
        {
            return;
        }

        AuditLogActionType actionType = entry.ActionType;

        if (ServiceThresholds.AntiBotThresholds.Thresholds.Contains(actionType))
        {
            await DetectBotAsync(member, actionType);
            return;
        }

        SuspectManager.IncrementViolation(member, actionType);

        await CheckThresholdAsync(member, actionType, guild);
    }

    /*
    if (databaseGuild.PunishmentRole.HasValue)
    {
        DiscordRole role = guild.GetRole(databaseGuild.PunishmentRole.Value);

        if (role != null)
        {
            await member.GrantRoleAsync(role, "Anti-Nuke first-level punishment");
        }
        else
        {
            await member.TimeoutAsync(DateTimeOffset.UtcNow.AddMinutes(10), "Anti-Nuke first-level punishment");
        }
    }
    else
    {
        await member.TimeoutAsync(DateTimeOffset.UtcNow.AddMinutes(10), "Anti-Nuke first-level punishment");
    }

    if (databaseGuild.PunishmentRole.HasValue)
    {
        DiscordRole role = guild.GetRole(databaseGuild.PunishmentRole.Value);

        if (role != null)
        {
            await member.GrantRoleAsync(role, "Anti-Nuke second-level punishment");
        }
        else
        {
            await member.TimeoutAsync(DateTimeOffset.UtcNow.AddHours(3), "Anti-Nuke second-level punishment");
        }
    }
    else
    {
        await member.TimeoutAsync(DateTimeOffset.UtcNow.AddHours(3), "Anti-Nuke second-level punishment");
    }
    */

    private async Task DetectBotAsync(DiscordMember member, AuditLogActionType actionType)
    {
        SuspectManager.IncrementViolation(member, actionType);

        int botThreshold = 5;
        int count = SuspectManager.GetViolationCount(member, actionType, ServiceThresholds.UniversalThresholds.TotalActionWindow);

        if (count >= botThreshold)
        {
            await member.BanAsync(reason: "Bot-like behavior detected");
            SuspectManager.RemoveSuspect(member);
        }
    }

    private static async Task ApplyPunishmentAsync(DiscordMember member, DiscordGuild guild, DatabaseGuild databaseGuild, string level, DateTimeOffset? timeout = null)
    {
        if (databaseGuild.PunishmentRole.HasValue)
        {
            DiscordRole role = guild.GetRole(databaseGuild.PunishmentRole.Value);
            if (role != null)
            {
                await member.GrantRoleAsync(role, $"Anti-Nuke {level} punishment");
                return;
            }
        }

        await member.TimeoutAsync(timeout ?? DateTimeOffset.UtcNow.AddMinutes(10), $"Anti-Nuke {level} punishment");
    }

    private async Task CheckThresholdAsync(DiscordMember member, AuditLogActionType actionType, DiscordGuild guild)
    {
        if (!ServiceThresholds.AntiUserThresholds.Thresholds.TryGetValue(actionType, out int threshold))
        {
            return;
        }

        TimeSpan window = actionType switch
        {
            AuditLogActionType.ChannelCreate or AuditLogActionType.ChannelDelete or AuditLogActionType.ChannelUpdate => ServiceThresholds.UniversalThresholds.ChannelTimeWindow,
            AuditLogActionType.RoleCreate or AuditLogActionType.RoleDelete or AuditLogActionType.RoleUpdate => ServiceThresholds.UniversalThresholds.RoleTimeWindow,
            AuditLogActionType.EmojiCreate or AuditLogActionType.EmojiDelete or AuditLogActionType.EmojiUpdate => ServiceThresholds.UniversalThresholds.EmojiTimeWindow,
            AuditLogActionType.StickerCreate or AuditLogActionType.StickerDelete or AuditLogActionType.StickerUpdate => ServiceThresholds.UniversalThresholds.StickerTimeWindow,
            AuditLogActionType.InviteCreate or AuditLogActionType.InviteDelete or AuditLogActionType.InviteUpdate => ServiceThresholds.UniversalThresholds.InviteTimeWindow,
            _ => ServiceThresholds.UniversalThresholds.TotalActionWindow
        };

        int count = SuspectManager.GetViolationCount(member, actionType, window);

        if (count < threshold)
        {
            return;
        }

        var databaseGuild = await guildRepository.TryGetAsync(guild.Id);
        if (databaseGuild is null || databaseGuild.ProtectionLevel >= ProtectionLevel.Minimal)
        {
            return;
        }

        if (count < threshold * 2)
        {
            await ApplyPunishmentAsync(member, guild, databaseGuild, "first-level", DateTimeOffset.UtcNow.AddMinutes(10));
        }
        else if (count < threshold * 3)
        {
            await ApplyPunishmentAsync(member, guild, databaseGuild, "second-level", DateTimeOffset.UtcNow.AddHours(3));
        }
        else
        {
            await member.RemoveAsync("Anti-Nuke third-level punishment");
            SuspectManager.RemoveSuspect(member);
        }
    }

    private async Task<bool> IsStaffAsync(DiscordMember member)
    {
        var staffUser = await staffUserRepository.TryGetAsync(member.Id);

        return staffUser is not null;
    }

    private async Task<bool> IsWhitelistedAsync(DiscordMember member)
    {
        var whitelistedUser = await whitelistedUserRepository.TryGetAsync(member.Id);

        return whitelistedUser is not null;
    }

    private async Task<bool> IsBlacklistedAsync(DiscordMember member)
    {
        var blacklistedUser = await blacklistedUserRepository.TryGetAsync(member.Id);

        return blacklistedUser is not null;
    }

    private async Task<bool> GuildExistsAsync(DiscordGuild guild)
    {
        var databaseGuild = await guildRepository.TryGetAsync(guild.Id);

        return databaseGuild is not null;
    }
}