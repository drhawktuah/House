using DSharpPlus;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Database;

public enum BackupReason
{
    Nuke,
    Reset,
    Manual
}

public static class BackupReasonExtensions
{
    public static string ToFriendlyString(this BackupReason reason)
    {
        return reason switch
        {
            BackupReason.Nuke => "Nuke Recovery",
            BackupReason.Reset => "Server Reset",
            BackupReason.Manual => "Manual Backup",
            _ => "Unknown"
        };
    }
}

public sealed class BackupOverwrite
{
    [BsonElement("target_id")]
    public ulong TargetId { get; set; }

    [BsonElement("target_type")]
    public OverwriteType TargetType { get; set; }

    [BsonElement("allow")]
    public long Allow { get; set; }

    [BsonElement("deny")]
    public long Deny { get; set; }
}

public sealed class BackupChannel : DatabaseEntity
{
    [BsonElement("guild_id")]
    public ulong GuildId { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("type")]
    public ChannelType Type { get; set; }

    [BsonElement("position")]
    public int Position { get; set; }

    [BsonElement("topic")]
    public string? Topic { get; set; }

    [BsonElement("bitrate")]
    public int? Bitrate { get; set; }

    [BsonElement("user_limit")]
    public int? UserLimit { get; set; }

    [BsonElement("rtc_region")]
    public string? RtcRegion { get; set; }

    [BsonElement("nsfw")]
    public bool? IsNsfw { get; set; }

    [BsonElement("slowmode")]
    public int? SlowMode { get; set; }

    [BsonElement("parent_id")]
    public ulong? ParentId { get; set; }

    [BsonElement("overwrites")]
    public List<BackupOverwrite> Overwrites { get; set; } = [];

    [BsonElement("children")]
    public List<BackupChannel>? Children { get; set; }
}

public sealed class BackupRole : DatabaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("color")]
    public int Color { get; set; }

    [BsonElement("position")]
    public int Position { get; set; }

    [BsonElement("permissions")]
    public long Permissions { get; set; }

    [BsonElement("hoist")]
    public bool IsHoisted { get; set; }

    [BsonElement("mentionable")]
    public bool IsMentionable { get; set; }

    [BsonElement("icon_data")]
    public byte[]? IconData { get; set; }

    [BsonElement("unicode_emoji")]
    public string? UnicodeEmoji { get; set; }
}

public sealed class BackupEmoji : DatabaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("animated")]
    public bool Animated { get; set; }

    [BsonElement("image_data")]
    public byte[]? ImageData { get; set; }
}

public sealed class BackupSticker : DatabaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("tags")]
    public string? Tags { get; set; }

    [BsonElement("format_type")]
    public int FormatType { get; set; }

    [BsonElement("image_data")]
    public byte[]? ImageData { get; set; }
}

public sealed class BackupGuild : DatabaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("icon_data")]
    public byte[]? IconData { get; set; }

    [BsonElement("owner_id")]
    public ulong OwnerId { get; set; }

    [BsonElement("roles")]
    public List<BackupRole> Roles { get; set; } = [];

    [BsonElement("channels")]
    public List<BackupChannel> Channels { get; set; } = [];

    [BsonElement("emojis")]
    public List<BackupEmoji> Emojis { get; set; } = [];

    [BsonElement("stickers")]
    public List<BackupSticker> Stickers { get; set; } = [];

    [BsonElement("reason")]
    public BackupReason Reason { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

/*
public class BackupEntity : DatabaseEntity
{

}

public sealed class BackupOverwrite : BackupEntity
{
    [BsonElement("target_id")]
    public ulong TargetID { get; set; }

    [BsonElement("type")]
    public OverwriteType TargetType { get; set; }

    [BsonElement("allow")]
    public long Allow { get; set; }

    [BsonElement("deny")]
    public long Deny { get; set; }
}

public abstract class BackupChannel : BackupEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("type")]
    public ChannelType Type { get; set; } = ChannelType.Text;

    [BsonElement("position")]
    public int Position { get; set; }

    [BsonElement("overwrites")]
    public List<BackupOverwrite> Overwrites { get; set; } = [];

    [BsonElement("parent")]
    public BackupCategoryChannel? Parent { get; set; }
}

public class BackupTextChannel : BackupChannel
{
    [BsonElement("topic")]
    public string? Topic { get; set; }

    [BsonElement("nsfw")]
    public bool IsNsfw { get; set; }

    [BsonElement("slowmode")]
    public int SlowMode { get; set; }
}

public class BackupVoiceChannel : BackupChannel
{
    [BsonElement("bitrate")]
    public int Bitrate { get; set; }

    [BsonElement("user_limit")]
    public int UserLimit { get; set; }

    [BsonElement("rtc_region")]
    public string? RtcRegion { get; set; }
}

public sealed class BackupCategoryChannel : BackupChannel
{
    [BsonElement("text_channels")]
    public List<BackupTextChannel> TextChannels { get; set; } = [];

    [BsonElement("voice_channels")]
    public List<BackupVoiceChannel> VoiceChannels { get; set; } = [];

    [BsonElement("forum_channels")]
    public List<BackupForumChannel> ForumChannels { get; set; } = [];

    [BsonElement("stage_channels")]
    public List<BackupStageChannel> StageChannels { get; set; } = [];
}

public sealed class BackupForumChannel : BackupChannel
{
    [BsonElement("guidelines")]
    public string? Guidelines { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = [];

    [BsonElement("default_reaction_emoji")]
    public string? DefaultReactionEmoji { get; set; }
}

public sealed class BackupStageChannel : BackupVoiceChannel
{
    [BsonElement("topic")]
    public string? Topic { get; set; }

    [BsonElement("is_live")]
    public bool IsLive { get; set; }
}

public sealed class BackupThreadChannel : BackupTextChannel
{
    [BsonElement("archived")]
    public bool IsArchived { get; set; }

    [BsonElement("owner_name")]
    public string? OwnerName { get; set; }

    [BsonElement("auto_archive_duration")]
    public int AutoArchiveDuration { get; set; }
}

public sealed class BackupRole : BackupEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("color")]
    public int Color { get; set; }

    [BsonElement("position")]
    public int Position { get; set; }

    [BsonElement("permissions")]
    public long Permissions { get; set; }

    [BsonElement("hoist")]
    public bool IsHoisted { get; set; }

    [BsonElement("mentionable")]
    public bool IsMentionable { get; set; }

    [BsonElement("icon_data")]
    public byte[]? IconData { get; set; }

    [BsonElement("unicode_emoji")]
    public string? UnicodeEmoji { get; set; }
}

public sealed class BackupEmoji : BackupEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("animated")]
    public bool Animated { get; set; }

    [BsonElement("image_data")]
    public byte[]? ImageData { get; set; }
}

public sealed class BackupSticker : BackupEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("tags")]
    public string? Tags { get; set; }

    [BsonElement("format_type")]
    public int FormatType { get; set; }

    [BsonElement("image_data")]
    public byte[]? ImageData { get; set; }
}

public sealed class BackupGuild : BackupEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("icon_data")]
    public byte[]? IconData { get; set; }

    [BsonElement("roles")]
    public List<BackupRole> Roles { get; set; } = [];

    [BsonElement("channels")]
    public List<BackupChannel> Channels { get; set; } = [];

    [BsonElement("emojis")]
    public List<BackupEmoji> Emojis { get; set; } = [];

    [BsonElement("stickers")]
    public List<BackupSticker> Stickers { get; set; } = [];
}
*/