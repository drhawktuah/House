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

    public void AddOrUpdate(DiscordMember member)
    {
        if (!suspects.TryGetValue(member.Id, out var suspect))
        {
            suspect = SuspectMember.Convert(member);
            suspects[member.Id] = suspect;
        }
    }

    public void IncrementViolation(DiscordMember member, AuditLogActionType actionType, int amount = 1)
    {
        AddOrUpdate(member);

        SuspectMember suspectMember = suspects[member.Id];
        suspectMember.AddViolation(actionType, amount);
    }

    public int GetViolationCount(DiscordMember member, AuditLogActionType actionType, TimeSpan window)
    {
        if (!suspects.TryGetValue(member.Id, out var suspect))
        {
            return 0;
        }

        return suspect.GetViolationCount(actionType, window);
    }

    public void RemoveSuspect(DiscordMember member) => suspects.Remove(member.Id);
}