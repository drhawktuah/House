using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using House.House.Attributes;
using House.House.Services.Database;
using House.House.Services.Protection;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using System.Text;
using System.Reflection.Metadata.Ecma335;

namespace House.House.Modules;

public sealed class BackupModule : BaseCommandModule
{
    public required BackupService BackupService { get; set; }
    public required BackupRepository BackupRepository { get; set; }

    [Command("backup")]
    [IsStaffOrOwner(Position.HeadAdmin)]
    [Cooldown(1, 1000, CooldownBucketType.Guild)]
    [RequireGuild]
    public async Task CreateBackupAsync(CommandContext context)
    {
        var interactivity = context.Client.GetInteractivity();

        var existingBackup = await BackupRepository.TryGetAsync(context.Guild.Id);
        var newBackup = await BackupService.CreateBackupAsync(context.Guild);

        if (existingBackup is null)
        {
            await BackupRepository.AddAsync(newBackup);

            DiscordEmbedBuilder embedBuilder = new()
            {
                Color = DiscordColor.SpringGreen,
                Title = "Backup Created",
                Description = $"A new backup for **{context.Guild.Name}** has been created",
                Timestamp = newBackup.CreatedAt
            };

            await context.RespondAsync(embedBuilder);
            return;
        }

        var roleDiffs = BackupDiffFormatter.Compare(
            existingBackup.Roles,
            newBackup.Roles,
            r => r.ID,
            r => [r.Name, r.Color.ToString(), r.Position.ToString(), r.Permissions.ToString()]
        );

        var emojiDiffs = BackupDiffFormatter.Compare(
            existingBackup.Emojis,
            newBackup.Emojis,
            e => e.ID,
            e => [e.Name, e.Animated.ToString()]
        );

        var stickerDiffs = BackupDiffFormatter.Compare(
            existingBackup.Stickers,
            newBackup.Stickers,
            s => s.ID,
            s => [s.Name, s.Description ?? "", s.Tags ?? "", s.FormatType.ToString()]
        );

        var channelDiffs = BackupDiffFormatter.Compare(
            existingBackup.Channels,
            newBackup.Channels,
            c => c.ID,
            c => [c.Name, c.Type.ToString(), c.Position.ToString()]
        );

        var formattedRoles = BackupDiffFormatter.FormatAnsi(roleDiffs, r => $"[Role] {r.Name}");
        var formattedEmojis = BackupDiffFormatter.FormatAnsi(emojiDiffs, e => $"[Emoji] {e.Name}");
        var formattedStickers = BackupDiffFormatter.FormatAnsi(stickerDiffs, s => $"[Sticker] {s.Name}");
        var formattedChannels = BackupDiffFormatter.FormatAnsi(channelDiffs, c => $"[Channel] {c.Name}");

        string diffSummary = string.Join("\n",
            new[]
            {
                formattedRoles is not "" ? "**Roles:**\n```ansi\n" + formattedRoles + "\n```" : "",
                formattedEmojis is not "" ? "**Emojis:**\n```ansi\n" + formattedEmojis + "\n```" : "",
                formattedStickers is not "" ? "**Stickers:**\n```ansi\n" + formattedStickers + "\n```" : "",
                formattedChannels is not "" ? "**Channels:**\n```ansi\n" + formattedChannels + "\n```" : ""
            }
            .Where(x => x != "")
        );

        if (string.IsNullOrWhiteSpace(diffSummary))
        {
            await context.RespondAsync("No changes were detected -- the current state matches the existing backup");
            return;
        }

        var confirmEmbed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Yellow)
            .WithTitle("Backup Exists")
            .WithDescription($"A backup already exists for **{context.Guild.Name}**.\n\n" + $"**Detected Changes:**\n{diffSummary}\n")
            .WithTimestamp(DateTime.UtcNow);

        confirmEmbed.WithFooter("React with ✅ to overwrite or ❌ to cancel", context.Client.CurrentUser.AvatarUrl);

        var message = await context.RespondAsync(confirmEmbed);

        await message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        await message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));

        var result = await interactivity.WaitForReactionAsync(x => x.User == context.User && x.Message == message && x.Emoji.Name == "✅" || x.Emoji.Name == "❌");

        if (result.TimedOut || result.Result.Emoji.Name == "❌")
        {
            await context.RespondAsync("Backup operation cancelled");
            return;
        }

        newBackup.ID = existingBackup.ID;

        await BackupRepository.UpdateAsync(newBackup);

        var successEmbed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.SpringGreen)
            .WithTitle("Backup Updated")
            .WithDescription($"The backup for **{context.Guild.Name}** has been updated successfully")
            .WithTimestamp(DateTime.UtcNow);

        await context.RespondAsync(successEmbed);
    }

    [Command("restorebackup")]
    [Description("Restore a previously created backup for this server")]
    [IsStaffOrOwner(Position.HeadAdmin)]
    [Cooldown(1, 1000, CooldownBucketType.Guild)]
    [RequireGuild]
    public async Task RestoreBackupAsync(CommandContext context)
    {
        var interactivity = context.Client.GetInteractivity();

        var backup = await BackupRepository.TryGetAsync(context.Guild.Id);
        if (backup is null)
        {
            await context.RespondAsync("No backup exists for this server.");
            return;
        }

        var confirmEmbed = new DiscordEmbedBuilder()
            .WithColor(DiscordColor.Orange)
            .WithTitle("Confirm Restore")
            .WithDescription($"Are you sure you want to restore the backup for **{context.Guild.Name}**?\nThis will overwrite the current server state!")
            .WithTimestamp(DateTime.UtcNow)
            .WithFooter("React with ✅ to confirm or ❌ to cancel", context.Client.CurrentUser.AvatarUrl);

        var message = await context.RespondAsync(confirmEmbed);

        await message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        await message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));

        var result = await interactivity.WaitForReactionAsync(
            x => x.User == context.User && x.Message == message && (x.Emoji.Name == "✅" || x.Emoji.Name == "❌")
        );

        if (result.TimedOut || result.Result.Emoji.Name == "❌")
        {
            await context.RespondAsync("Restore operation cancelled.");
            return;
        }

        await BackupService.RestoreBackupAsync(context.Guild, backup);
    }
}

public enum DiffChangeType
{
    Added,
    Removed,
    Modified
}

public sealed class BackupDiffResult<T>
{
    public required ulong Key { get; init; }
    public required T? OldItem { get; init; }
    public required T? NewItem { get; init; }
    public required DiffChangeType? ChangeType { get; init; }
}

public static class BackupDiffFormatter
{
    public static List<BackupDiffResult<T>> Compare<T>(IEnumerable<T> oldValues, IEnumerable<T> newValues, Func<T, ulong> idSelector, Func<T, string[]> valueSelector)
    {
        ArgumentNullException.ThrowIfNull(oldValues);
        ArgumentNullException.ThrowIfNull(newValues);

        var oldDict = oldValues.ToDictionary(idSelector, v => v);
        var newDict = newValues.ToDictionary(idSelector, v => v);

        List<BackupDiffResult<T>> diffResults = [];

        foreach (var (id, oldItem) in oldDict)
        {
            if (newDict.TryGetValue(id, out var newItem))
            {
                var oldObjects = valueSelector(oldItem);
                var newObjects = valueSelector(newItem);

                if (!oldValues.SequenceEqual(newValues))
                {
                    diffResults.Add(new BackupDiffResult<T>
                    {
                        Key = id,
                        OldItem = oldItem,
                        NewItem = newItem,
                        ChangeType = DiffChangeType.Modified
                    });
                }
            }
            else
            {
                diffResults.Add(new BackupDiffResult<T>
                {
                    Key = id,
                    OldItem = oldItem,
                    NewItem = default,
                    ChangeType = DiffChangeType.Removed
                });
            }
        }

        foreach (var (id, newItem) in newDict)
        {
            if (!oldDict.ContainsKey(id))
            {
                diffResults.Add(new BackupDiffResult<T>
                {
                    Key = id,
                    OldItem = default,
                    NewItem = newItem,
                    ChangeType = DiffChangeType.Added
                });
            }
        }

        return diffResults;
    }

    public static string FormatAnsi<T>(IEnumerable<BackupDiffResult<T>> diffResults, Func<T, string> displaySelector)
    {
        StringBuilder builder = new();

        foreach (BackupDiffResult<T> diffResult in diffResults)
        {
            string line = diffResult.ChangeType switch
            {
                DiffChangeType.Added => $"\u001b[32m+ {displaySelector(diffResult.NewItem!)}\u001b[0m",
                DiffChangeType.Removed => $"\u001b[31m- {displaySelector(diffResult.OldItem!)}\u001b[0m",
                DiffChangeType.Modified => $"\u001b[33m~ {displaySelector(diffResult.NewItem!)}\u001b[0m",
                _ => displaySelector(diffResult.NewItem ?? diffResult.OldItem!),
            };

            builder.AppendLine(line);
        }

        return builder.ToString();
    }
}