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

        var everyoneRole = guild.EveryoneRole;

        try
        {
            await Task.WhenAll(guild.Roles.Values.Where(r => r != everyoneRole && !r.IsManaged).Select(r => r.DeleteAsync()));
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

                    try
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
                    catch
                    {
                        continue;
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