using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace House.House.Services.Protection;

public sealed class SuspectManager
{
    public IReadOnlyDictionary<ulong, SuspectMember> Suspects => suspects;
    private readonly Dictionary<ulong, SuspectMember> suspects = [];

    public void AddSuspect(SuspectMember member)
    {
        suspects.Add(member.ID, member);
    }

    public void AddSuspect(DiscordMember member)
    {
        SuspectMember suspectMember = SuspectMember.Convert(member);

        suspects.Add(member.Id, suspectMember);
    }

    public void AddSuspect(DiscordMember member, Dictionary<AuditLogActionType, int>? violations = null)
    {
        SuspectMember suspectMember = new()
        {
            ID = member.Id,
            Username = member.Username,
            Violations = violations ?? []
        };

        suspects.Add(member.Id, suspectMember);
    }

    public void RemoveSuspect(DiscordMember member)
    {
        suspects.Remove(member.Id);
    }

    public void RemoveSuspect(SuspectMember member)
    {
        suspects.Remove(member.ID);
    }
}