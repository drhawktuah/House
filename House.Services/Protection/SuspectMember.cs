using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using House.House.Services.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Protection;

public class SuspectMember : DatabaseUser
{
    [BsonElement("violations")]
    public Dictionary<AuditLogActionType, List<DateTime>> Violations { get; set; } = [];

    public static SuspectMember Convert(DiscordMember member)
    {
        return new()
        {
            ID = member.Id,
            Username = member.Username,
            CreatedAt = DateTime.Now
        };
    }

    public void AddViolation(AuditLogActionType actionType, int amount = 1)
    {
        if (!Violations.TryGetValue(actionType, out List<DateTime>? times))
        {
            times = [];
            Violations[actionType] = times;
        }

        for (int i = 0; i < amount; i++)
        {
            times.Add(DateTime.UtcNow);
        }
    }

    public int GetViolationCount(AuditLogActionType actionType, TimeSpan timeWindow)
    {
        if (!Violations.TryGetValue(actionType, out List<DateTime>? times))
        {
            return 0;
        }

        times.RemoveAll(ts => ts + timeWindow < DateTime.UtcNow);
        return times.Count;
    }
}