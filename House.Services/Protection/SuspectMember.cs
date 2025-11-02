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
    public Dictionary<AuditLogActionType, int> Violations { get; set; } = [];

    public static SuspectMember Convert(DiscordMember member)
    {
        return new()
        {
            ID = member.Id,
            Username = member.Username,
            CreatedAt = DateTime.Now
        };
    }
}