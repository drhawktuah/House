using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using House.House.Extensions;
using House.House.Services.Database;
using MongoDB.Driver;

namespace House.House.Services.Protection;

/*
public sealed class BackupService
{
    private readonly BackupRepository backupRepository;
    private readonly DiscordClient client;

    public BackupService(DiscordClient client, BackupRepository backupRepository)
    {
        this.client = client;
        this.backupRepository = backupRepository;
    }

    public async Task<BackupGuild> CreateBackupAsync(DiscordGuild guild)
    {
        BackupGuild backup = new()
        {
            ID = guild.Id,
            Name = guild.Name,
            CreatedAt = DateTime.UtcNow,
        };

        backup.Roles = guild.Roles.Values.Select(r => new BackupRole
        {
            ID = r.Id,
            Name = r.Name,
            Color = r.Color.Value,
            Position = r.Position,
            Permissions = (long)r.Permissions,
            IsHoisted = r.IsHoisted,
            IsMentionable = r.IsMentionable,
            UnicodeEmoji = r.Emoji
        }).ToList();

        backup.Emojis = guild.Emojis.Values.Select(e => new BackupEmoji
        {
            ID = e.Id,
            Name = e.Name,
            Animated = e.IsAnimated,
            ImageData = e.
        }).ToList();

        // Backup Stickers
        backup.Stickers = guild.Stickers.Values.Select(s => new BackupSticker
        {
            ID = s.Id,
            Name = s.Name,
            Description = s.Description,
            Tags = string.Join(' ', s.Tags),
            FormatType = (int)s.FormatType,
            ImageData = await DiscordImageHelper.GetSafeImageBytesAsync(s)
        }).ToList();

        // Backup Channels
        backup.Channels = [];
    }
}
*/

/*
public sealed class BackupService
{
    private readonly DiscordClient client;
    private readonly BackupRepository backupRepository;

    public BackupService(DiscordClient client, BackupRepository backupRepository)
    {
        this.client = client;
        this.backupRepository = backupRepository;
    }

    public async Task<BackupGuild> CreateBackupAsync(DiscordGuild guild)
    {
        var backup = await backupRepository.GetOrCreateAsync(
            guild.Id,
            async id => new BackupGuild
            {
                ID = id,
                Name = guild.Name,
                IconData = await DiscordImageHelper.GetSafeImageBytesAsync(guild)
            },
            async existing =>
            {
                existing.Name = guild.Name;
                existing.IconData = await DiscordImageHelper.GetSafeImageBytesAsync(guild);
                return existing;
            }
        );

        await CreateOrUpdateBackupRolesAsync(guild, backup);
        await CreateOrUpdateBackupEmojisAsync(guild, backup);
        await CreateOrUpdateBackupStickersAsync(guild, backup);

        backup.Channels = CreateBackupChannels(guild);

        await backupRepository.UpdateAsync(backup);

        return backup;
    }

    private static List<BackupChannel> CreateBackupChannels(DiscordGuild guild)
    {
        List<BackupChannel> backupChannels = [];

        if (guild.Channels is null || guild.Channels.Count == 0)
        {
            return backupChannels;
        }

        var channels = guild.Channels.Values
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Id)
            .ToList();

        Dictionary<ulong, BackupCategoryChannel> categories = [];

        foreach (var channel in channels)
        {
            if (channel is null)
            {
                continue;
            }

            var overwrites = CreateOverwrites(channel);
            BackupChannel? backupChannel = null;

            switch (channel.Type)
            {
                case ChannelType.Category:
                    {
                        var backupCategory = new BackupCategoryChannel
                        {
                            ID = channel.Id,
                            Name = channel.Name ?? "Unnamed Category",
                            Type = channel.Type,
                            Position = channel.Position,
                            Overwrites = overwrites
                        };

                        categories[channel.Id] = backupCategory;
                        backupChannel = backupCategory;
                        break;
                    }

                case ChannelType.Text:
                    {
                        var backupText = new BackupTextChannel
                        {
                            ID = channel.Id,
                            Name = channel.Name ?? "Unnamed Text Channel",
                            Type = channel.Type,
                            Position = channel.Position,
                            Topic = channel.Topic,
                            IsNsfw = channel.IsNSFW,
                            SlowMode = channel.PerUserRateLimit ?? -1,
                            Overwrites = overwrites
                        };

                        if (channel.ParentId.HasValue && categories.TryGetValue(channel.ParentId.Value, out var parent))
                        {
                            backupText.Parent = parent;
                            parent.TextChannels.Add(backupText);
                        }

                        backupChannel = backupText;
                        break;
                    }

                case ChannelType.Voice:
                    {
                        var backupVoice = new BackupVoiceChannel
                        {
                            ID = channel.Id,
                            Name = channel.Name ?? "Unnamed Voice Channel",
                            Type = channel.Type,
                            Position = channel.Position,
                            Bitrate = channel.Bitrate ?? 0,
                            UserLimit = channel.UserLimit ?? -1,
                            RtcRegion = channel.RtcRegion?.Name,
                            Overwrites = overwrites
                        };

                        if (channel.ParentId.HasValue && categories.TryGetValue(channel.ParentId.Value, out var parent))
                        {
                            backupVoice.Parent = parent;
                            parent.VoiceChannels.Add(backupVoice);
                        }

                        backupChannel = backupVoice;
                        break;
                    }

                default:
                    continue;
            }

            if (backupChannel is not null)
            {
                backupChannels.Add(backupChannel);
            }
        }

        return backupChannels;
    }

    private static async Task<List<BackupRole>> CreateOrUpdateBackupRolesAsync(DiscordGuild guild, BackupGuild backup)
    {
        var roles = guild.Roles.Values
            .Where(r => !r.IsManaged)
            .OrderBy(r => r.Position)
            .ToList();

        var updatedRoles = new List<BackupRole>();

        foreach (var role in roles)
        {
            var existing = backup.Roles.FirstOrDefault(r => r.ID == role.Id);
            var iconBytes = await DiscordImageHelper.GetSafeImageBytesAsync(role);

            if (existing is null)
            {
                existing = new BackupRole { ID = role.Id };
                backup.Roles.Add(existing);
            }

            existing.Name = role.Name;
            existing.Color = role.Color.Value;
            existing.Position = role.Position;
            existing.Permissions = (long)role.Permissions;
            existing.IsHoisted = role.IsHoisted;
            existing.IsMentionable = role.IsMentionable;
            existing.IconData = iconBytes;
            existing.UnicodeEmoji = role.Emoji?.GetDiscordName();

            updatedRoles.Add(existing);
        }

        return updatedRoles;
    }

    private static async Task<List<BackupEmoji>> CreateOrUpdateBackupEmojisAsync(DiscordGuild guild, BackupGuild backup)
    {
        var emojis = guild.Emojis.Values.ToList();
        var updatedEmojis = new List<BackupEmoji>();

        foreach (var emoji in emojis)
        {
            var existing = backup.Emojis.FirstOrDefault(e => e.ID == emoji.Id);
            var imageBytes = await DiscordImageHelper.GetSafeImageBytesAsync(emoji);

            if (existing is null)
            {
                existing = new BackupEmoji { ID = emoji.Id };
                backup.Emojis.Add(existing);
            }

            existing.Name = emoji.Name;
            existing.Animated = emoji.IsAnimated;
            existing.ImageData = imageBytes;

            updatedEmojis.Add(existing);
        }

        return updatedEmojis;
    }

    private static async Task<List<BackupSticker>> CreateOrUpdateBackupStickersAsync(DiscordGuild guild, BackupGuild backup)
    {
        var stickers = guild.Stickers.Values.ToList();
        var updatedStickers = new List<BackupSticker>();

        foreach (var sticker in stickers)
        {
            var existing = backup.Stickers.FirstOrDefault(s => s.ID == sticker.Id);
            var imageBytes = await DiscordImageHelper.GetSafeImageBytesAsync(sticker);

            if (existing is null)
            {
                existing = new BackupSticker { ID = sticker.Id };
                backup.Stickers.Add(existing);
            }

            existing.Name = sticker.Name;
            existing.Description = sticker.Description;
            existing.Tags = string.Join(' ', sticker.Tags);
            existing.FormatType = (int)sticker.FormatType;
            existing.ImageData = imageBytes;

            updatedStickers.Add(existing);
        }

        return updatedStickers;
    }
    private static List<BackupOverwrite> CreateOverwrites(DiscordChannel channel)
    {
        List<BackupOverwrite> overwrites = [];

        if (channel.PermissionOverwrites.Count == 0)
        {
            return [];
        }

        foreach (DiscordOverwrite overwrite in channel.PermissionOverwrites)
        {
            overwrites.Add(new BackupOverwrite
            {
                TargetType = overwrite.Type,
                Allow = (long)overwrite.Allowed,
                Deny = (long)overwrite.Denied
            });
        }

        return overwrites;
    }
}
*/

public sealed class BackupService
{
    private readonly BackupRepository backupRepository;

    public BackupService(BackupRepository backupRepository)
    {
        this.backupRepository = backupRepository;
    }

    public async Task<BackupGuild> CreateBackupAsync(DiscordGuild guild)
    {
        var backup = await backupRepository.GetOrCreateAsync(
            guild.Id,
            async id => new BackupGuild
            {
                ID = id,
                Name = guild.Name,
                IconData = await DiscordImageHelper.GetSafeImageBytesAsync(guild)
            },
            async existing =>
            {
                existing.Name = guild.Name;
                existing.IconData = await DiscordImageHelper.GetSafeImageBytesAsync(guild);
                return existing;
            }
        );

        backup.Roles = await CreateOrUpdateBackupRolesAsync(guild, backup);
        backup.Emojis = await CreateOrUpdateBackupEmojisAsync(guild, backup);
        backup.Stickers = await CreateOrUpdateBackupStickersAsync(guild, backup);

        backup.Channels = CreateBackupChannels(guild);

        await backupRepository.UpdateAsync(backup);

        return backup;
    }

    public async Task<BackupGuild> RestoreBackupAsync(DiscordGuild guild, BackupGuild backup)
    {
        ArgumentNullException.ThrowIfNull(guild);
        ArgumentNullException.ThrowIfNull(backup);

        try
        {
            await Task.WhenAll(guild.Roles.Values.Where(r => r.Name != "@everyone" && !r.IsManaged).Select(r => r.DeleteAsync()));
            await Task.WhenAll(guild.Channels.Values.Select(c => c.DeleteAsync()));

            await Task.WhenAll(
                guild.Emojis.Values.Select(async e =>
                {
                    var emoji = await guild.GetEmojiAsync(e.Id);
                    return guild.DeleteEmojiAsync(emoji);
                })
            );

            await Task.WhenAll(
                guild.Stickers.Values.Select(s => guild.DeleteStickerAsync(s))
            );

            foreach (var roleData in backup.Roles.OrderBy(r => r.Position))
            {
                if (roleData.Name == "@everyone")
                {
                    continue;
                }

                var role = await guild.CreateRoleAsync(
                    name: roleData.Name,
                    permissions: (Permissions)roleData.Permissions,
                    color: new DiscordColor(roleData.Color),
                    hoist: roleData.IsHoisted,
                    mentionable: roleData.IsMentionable,
                    reason: "Restoring backup"
                );

                if (roleData.IconData != null)
                {
                    using var ms = new MemoryStream(roleData.IconData);
                    await role.ModifyAsync(x => x.Icon = ms);
                }

                if (!string.IsNullOrEmpty(roleData.UnicodeEmoji))
                {
                    await role.ModifyAsync(x => x.Emoji = DiscordEmoji.FromUnicode(roleData.UnicodeEmoji));
                }
            }

            var categoryMap = new Dictionary<ulong, DiscordChannel>();
            foreach (var catBackup in backup.Channels.Where(c => c.Type == ChannelType.Category).OrderBy(c => c.Position))
            {
                var cat = await guild.CreateChannelAsync(
                    catBackup.Name,
                    ChannelType.Category,
                    position: catBackup.Position
                );

                categoryMap[catBackup.ID] = cat;
            }

            foreach (var chBackup in backup.Channels.Where(c => c.Type != ChannelType.Category).OrderBy(c => c.Position))
            {
                DiscordChannel? parent = null;
                if (chBackup.ParentId.HasValue && categoryMap.TryGetValue(chBackup.ParentId.Value, out var cat))
                {
                    parent = cat;
                }

                var channel = await guild.CreateChannelAsync(
                    chBackup.Name,
                    chBackup.Type,
                    parent,
                    topic: chBackup.Topic,
                    bitrate: chBackup.Bitrate ?? 64000,
                    userLimit: chBackup.UserLimit ?? 0,
                    nsfw: chBackup.IsNsfw ?? false,
                    position: chBackup.Position
                );

                foreach (var overwrite in chBackup.Overwrites)
                {
                    var role = guild.GetRole(overwrite.TargetId);
                    if (role != null)
                    {
                        await channel.AddOverwriteAsync(
                            role,
                            allow: (Permissions)overwrite.Allow,
                            deny: (Permissions)overwrite.Deny
                        );
                    }
                    else
                    {
                        DiscordMember member = await guild.GetMemberAsync(overwrite.TargetId);
                        if (member != null)
                        {
                            await channel.AddOverwriteAsync(
                                member,
                                allow: (Permissions)overwrite.Allow,
                                deny: (Permissions)overwrite.Deny
                            );
                        }
                    }
                }
            }

            foreach (var emojiData in backup.Emojis)
            {
                if (emojiData.ImageData == null)
                {
                    continue;
                }

                using var ms = new MemoryStream(emojiData.ImageData);
                await guild.CreateEmojiAsync(emojiData.Name, ms, reason: $"Restoring backup: {backup.Reason.ToFriendlyString()}");
            }

            foreach (var stickerData in backup.Stickers)
            {
                if (stickerData.ImageData == null)
                {
                    continue;
                }

                using var ms = new MemoryStream(stickerData.ImageData);
                await guild.CreateStickerAsync(
                    name: stickerData.Name,
                    description: stickerData.Description,
                    tags: stickerData.Tags,
                    format: (StickerFormat)stickerData.FormatType,
                    imageContents: ms,
                    reason: $"Restoring backup: {backup.Reason.ToFriendlyString()}"
                );
            }

            if (backup.IconData != null)
            {
                using var ms = new MemoryStream(backup.IconData);
                await guild.ModifyAsync(g =>
                {
                    g.Icon = ms;
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return backup;
    }

    private static List<BackupChannel> CreateBackupChannels(DiscordGuild guild)
    {
        var backupChannels = new List<BackupChannel>();
        if (guild.Channels is null)
        {
            return backupChannels;
        }

        var categories = new Dictionary<ulong, BackupChannel>();

        foreach (var channel in guild.Channels.Values.OrderBy(c => c.Position))
        {
            if (channel is null)
            {
                continue;
            }

            var backupChannel = new BackupChannel
            {
                ID = channel.Id,
                Name = channel.Name ?? "Unnamed Channel",
                Type = channel.Type,
                Position = channel.Position,
                Overwrites = CreateOverwrites(channel),
                ParentId = channel.ParentId
            };

            switch (channel.Type)
            {
                case ChannelType.Text:
                    backupChannel.Topic = channel.Topic;
                    backupChannel.IsNsfw = channel.IsNSFW;
                    backupChannel.SlowMode = channel.PerUserRateLimit ?? 0;
                    break;

                case ChannelType.Voice:
                    backupChannel.Bitrate = channel.Bitrate ?? 0;
                    backupChannel.UserLimit = channel.UserLimit ?? 0;
                    backupChannel.RtcRegion = channel.RtcRegion?.Name;
                    break;

                case ChannelType.Category:
                    categories[channel.Id] = backupChannel;
                    break;
            }

            if (channel.ParentId.HasValue && categories.TryGetValue(channel.ParentId.Value, out var parent))
            {
                parent.Children ??= [];
                parent.Children.Add(backupChannel);
            }

            backupChannels.Add(backupChannel);
        }

        return backupChannels;
    }

    private static async Task<List<BackupRole>> CreateOrUpdateBackupRolesAsync(DiscordGuild guild, BackupGuild backup)
    {
        var updatedRoles = new List<BackupRole>();

        foreach (var role in guild.Roles.Values.Where(r => !r.IsManaged).OrderBy(r => r.Position))
        {
            var existing = backup.Roles.FirstOrDefault(r => r.ID == role.Id) ?? new BackupRole { ID = role.Id };

            existing.Name = role.Name;
            existing.Color = role.Color.Value;
            existing.Position = role.Position;
            existing.Permissions = (long)role.Permissions;
            existing.IsHoisted = role.IsHoisted;
            existing.IsMentionable = role.IsMentionable;
            existing.IconData = await DiscordImageHelper.GetSafeImageBytesAsync(role);
            existing.UnicodeEmoji = role.Emoji?.GetDiscordName();

            if (!backup.Roles.Contains(existing))
            {
                backup.Roles.Add(existing);
            }

            updatedRoles.Add(existing);
        }

        return updatedRoles;
    }

    private static async Task<List<BackupEmoji>> CreateOrUpdateBackupEmojisAsync(DiscordGuild guild, BackupGuild backup)
    {
        var updatedEmojis = new List<BackupEmoji>();

        foreach (var emoji in guild.Emojis.Values)
        {
            var existing = backup.Emojis.FirstOrDefault(e => e.ID == emoji.Id) ?? new BackupEmoji { ID = emoji.Id };

            existing.Name = emoji.Name;
            existing.Animated = emoji.IsAnimated;
            existing.ImageData = await DiscordImageHelper.GetSafeImageBytesAsync(emoji);

            if (!backup.Emojis.Contains(existing))
            {
                backup.Emojis.Add(existing);
            }

            updatedEmojis.Add(existing);
        }

        return updatedEmojis;
    }

    private static async Task<List<BackupSticker>> CreateOrUpdateBackupStickersAsync(DiscordGuild guild, BackupGuild backup)
    {
        var updatedStickers = new List<BackupSticker>();

        foreach (var sticker in guild.Stickers.Values)
        {
            var existing = backup.Stickers.FirstOrDefault(s => s.ID == sticker.Id) ?? new BackupSticker { ID = sticker.Id };

            existing.Name = sticker.Name;
            existing.Description = sticker.Description;
            existing.Tags = string.Join(' ', sticker.Tags);
            existing.FormatType = (int)sticker.FormatType;
            existing.ImageData = await DiscordImageHelper.GetSafeImageBytesAsync(sticker);

            if (!backup.Stickers.Contains(existing))
            {
                backup.Stickers.Add(existing);
            }

            updatedStickers.Add(existing);
        }

        return updatedStickers;
    }

    private static List<BackupOverwrite> CreateOverwrites(DiscordChannel channel)
    {
        return channel.PermissionOverwrites
            .Select(o => new BackupOverwrite
            {
                TargetId = o.Id,
                TargetType = o.Type,
                Allow = (long)o.Allowed,
                Deny = (long)o.Denied
            })
            .ToList();
    }
}