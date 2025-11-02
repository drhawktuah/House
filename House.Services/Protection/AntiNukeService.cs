using System;
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
    private readonly DiscordClient client;

    private readonly WhitelistedUserRepository whitelistedUserRepository;
    private readonly BlacklistedUserRepository blacklistedUserRepository;
    private readonly SuspectMemberRepository suspectMemberRepository;
    private readonly GuildRepository guildRepository;
    private readonly StaffUserRepository staffUserRepository;

    public AntiNukeService(DiscordClient client)
    {
        this.client = client;

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
        if (await IsStaffAsync((DiscordMember)message.Author))
        {
            return;
        }

        var guild = message.Channel.Guild;

        if (!await GuildExistsAsync(guild))
        {
            return;
        }
    }

    public async Task HandleViolationsAsync(DiscordMember member)
    {
        
    }

    private async Task<bool> IsSuspectAsync(DiscordMember member)
    {
        var suspectMember = await suspectMemberRepository.TryGetAsync(member.Id);

        return suspectMember is not null;
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