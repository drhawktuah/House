using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace House.House.Services.Protection;

public static class ServiceThresholds
{
    public static class UniversalThresholds
    {
        public const int SpamThreshold = 5;
        public const int RemoveMemberThreshold = 3;
        public const int ChannelThreshold = 2;
        public const int RoleThreshold = 2;
        public const int EmojiThreshold = 3;
        public const int StickerThreshold = 3;
        public const int InviteThreshold = 2;

        public static readonly TimeSpan SpamTimeWindow = TimeSpan.FromSeconds(5);
        public static readonly TimeSpan RemoveMemberTimeWindow = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan ChannelTimeWindow = TimeSpan.FromSeconds(20);
        public static readonly TimeSpan RoleTimeWindow = TimeSpan.FromSeconds(20);
        public static readonly TimeSpan EmojiTimeWindow = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan StickerTimeWindow = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan InviteTimeWindow = TimeSpan.FromSeconds(5);

        public static readonly TimeSpan TotalActionWindow = TimeSpan.FromMinutes(10);
    }

    public static class AntiBotThresholds
    {
        public static readonly HashSet<AuditLogActionType> Thresholds =
        [
            AuditLogActionType.ChannelCreate,
            AuditLogActionType.ChannelDelete,
            AuditLogActionType.ChannelUpdate,
            AuditLogActionType.OverwriteCreate,
            AuditLogActionType.OverwriteUpdate,
            AuditLogActionType.OverwriteDelete,

            AuditLogActionType.Prune,
            AuditLogActionType.Ban,
            AuditLogActionType.Kick,
            AuditLogActionType.MemberUpdate,
            AuditLogActionType.MemberRoleUpdate,
            AuditLogActionType.MemberMove,
            AuditLogActionType.MemberDisconnect,

            AuditLogActionType.RoleCreate,
            AuditLogActionType.RoleDelete,
            AuditLogActionType.RoleUpdate,

            AuditLogActionType.InviteCreate,
            AuditLogActionType.InviteDelete,
            AuditLogActionType.InviteUpdate,

            AuditLogActionType.WebhookCreate,
            AuditLogActionType.WebhookDelete,
            AuditLogActionType.WebhookUpdate,

            AuditLogActionType.EmojiCreate,
            AuditLogActionType.EmojiDelete,
            AuditLogActionType.EmojiUpdate,
            AuditLogActionType.StickerCreate,
            AuditLogActionType.StickerDelete,
            AuditLogActionType.StickerUpdate,

            AuditLogActionType.IntegrationCreate,
            AuditLogActionType.IntegrationDelete,
            AuditLogActionType.IntegrationUpdate,

            AuditLogActionType.GuildUpdate,
            AuditLogActionType.GuildScheduledEventCreate,
            AuditLogActionType.GuildScheduledEventUpdate,
            AuditLogActionType.GuildScheduledEventDelete,

            AuditLogActionType.ThreadCreate,
            AuditLogActionType.ThreadDelete,
            AuditLogActionType.ThreadUpdate
        ];
    }

    public static class AntiUserThresholds
    {
        public static readonly Dictionary<AuditLogActionType, int> Thresholds = new()
        {
            { AuditLogActionType.ChannelCreate, 1 },
            { AuditLogActionType.ChannelDelete, 1 },
            { AuditLogActionType.ChannelUpdate, 1 },
            { AuditLogActionType.OverwriteCreate, 1 },
            { AuditLogActionType.OverwriteUpdate, 1 },
            { AuditLogActionType.OverwriteDelete, 1 },

            { AuditLogActionType.Prune, 1 },
            { AuditLogActionType.Ban, 1 },
            { AuditLogActionType.Kick, 1 },
            { AuditLogActionType.MemberUpdate, 1 },

            { AuditLogActionType.MemberRoleUpdate, 1 },
            { AuditLogActionType.MemberMove, 3 },
            { AuditLogActionType.MemberDisconnect, 3 },

            { AuditLogActionType.RoleCreate, 1 },
            { AuditLogActionType.RoleDelete, 1 },
            { AuditLogActionType.RoleUpdate, 1 },

            { AuditLogActionType.InviteCreate, 5 },
            { AuditLogActionType.InviteDelete, 5 },
            { AuditLogActionType.InviteUpdate, 5 },

            { AuditLogActionType.WebhookCreate, 1 },
            { AuditLogActionType.WebhookDelete, 1 },
            { AuditLogActionType.WebhookUpdate, 1 },

            { AuditLogActionType.EmojiCreate, 3 },
            { AuditLogActionType.EmojiDelete, 2 },
            { AuditLogActionType.EmojiUpdate, 3 },

            { AuditLogActionType.StickerCreate, 3 },
            { AuditLogActionType.StickerDelete, 2 },
            { AuditLogActionType.StickerUpdate, 3 },

            { AuditLogActionType.IntegrationCreate, 1 },
            { AuditLogActionType.IntegrationDelete, 1 },
            { AuditLogActionType.IntegrationUpdate, 1 },

            { AuditLogActionType.GuildUpdate, 1 },
            { AuditLogActionType.GuildScheduledEventCreate, 5 },
            { AuditLogActionType.GuildScheduledEventUpdate, 5 },
            { AuditLogActionType.GuildScheduledEventDelete, 3 },

            { AuditLogActionType.ThreadCreate, 3 },
            { AuditLogActionType.ThreadDelete, 3 },
            { AuditLogActionType.ThreadUpdate, 3 }
        };
    }
}