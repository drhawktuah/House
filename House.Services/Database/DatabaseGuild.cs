using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public class DatabaseGuild : DatabaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("default_channel_id")]
    public ulong? DefaultChannelID { get; set; }

    [BsonElement("punishment_role")]
    public ulong? PunishmentRole { get; set; }

    [BsonElement("starboard_channel_id")]
    public ulong? StarboardChannelID { get; set; }

    [BsonElement("protection_level")]
    public ProtectionLevel ProtectionLevel { get; set; } = ProtectionLevel.Basic;
}