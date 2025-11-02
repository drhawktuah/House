using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace House.House.Services.Database;

public class DatabaseGuild : DatabaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public ulong? DefaultChannelID { get; set; }
    public ulong? PunishmentRole { get; set; }
    public ulong? StarboardChannelID { get; set; }

    public ProtectionLevel ProtectionLevel { get; set; } = ProtectionLevel.Basic;
}