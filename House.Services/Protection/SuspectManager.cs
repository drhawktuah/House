using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace House.House.Services.Protection;

public sealed class SuspectManager
{
    private ConcurrentDictionary<ulong, SuspectMember> suspects = [];

    public SuspectMember AddOrUpdate(DiscordMember member)
    {
        return suspects.AddOrUpdate(member.Id, id => SuspectMember.Convert(member), (id, existing) => existing);
    }

    public void IncrementViolation(DiscordMember member, AuditLogActionType actionType, int amount = 1)
    {
        SuspectMember suspectMember = AddOrUpdate(member);
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

    public void RemoveSuspect(DiscordMember member) => suspects.TryRemove(member.Id, out _);
}