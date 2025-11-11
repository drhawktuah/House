using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using House.House.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace House.House.Modules;

[Description("House's general module")]
public sealed class GeneralModule : BaseCommandModule
{
    [Command("avatar")]
    [Aliases("pfp", "av", "profilepicture", "getavatar", "getpfp")]
    [Description("Gets the avatar of a user")]
    [Cooldown(2, 10, CooldownBucketType.User)]
    [RequireGuild]
    public async Task GetAvatarAsync(CommandContext context, DiscordUser? user = null)
    {
        user ??= context.User;

        DiscordEmbedBuilder builder = new()
        {
            Title = $"{user.Username}'s avatar",
            ImageUrl = user.GetAvatarUrl(DSharpPlus.ImageFormat.Png, 4096),
            Footer = new DiscordEmbedBuilder.EmbedFooter()
            {
                Text = "use h.av or h.avatar to obtain your avatar or another user's",
                IconUrl = context.Client.CurrentUser.AvatarUrl
            },
            Color = EmbedUtils.EmbedColor
        };

        await context.RespondAsync(builder);
    }

    [Command("whois")]
    [Description("Gets information about a user in the server")]
    [Cooldown(2, 10, CooldownBucketType.User)]
    [RequireGuild]
    public async Task WhoisAsync(CommandContext context, DiscordMember? member = null)
    {
        member ??= context.Member!;
        DiscordMember user = member;

        StringBuilder sb = new()
        {
            Capacity = 512
        };

        sb.AppendLine($"ID: `{user.Id}`");
        sb.AppendLine($"Username: `{user.Username}`");
        sb.AppendLine($"Nickname: `{user.Nickname ?? "none"}`");
        sb.AppendLine($"Account created: `{user.CreationTimestamp.UtcDateTime:yyyy-MM-dd HH:mm:ss}`");
        sb.AppendLine($"Joined server: `{user.JoinedAt.UtcDateTime:yyyy-MM-dd HH:mm:ss}`");

        if (user.PremiumSince.HasValue)
        {
            sb.AppendLine($"Boosting since: `{user.PremiumSince.Value.UtcDateTime:yyyy-MM-dd HH:mm:ss}`");
        }

        var roles = user.Roles
            .OrderByDescending(r => r.Position)
            .Select(r => r.Mention)
            .ToList();

        string roleDisplay = roles.Count == 0 ? "`none`" : string.Join(", ", roles);

        DiscordEmbedBuilder embed = new()
        {
            Color = EmbedUtils.EmbedColor,
            Title = user.Username,
            Description = sb.ToString()
        };

        embed.AddField("Roles", roleDisplay, false);
        embed.AddField("Highest Role", user.Roles.OrderByDescending(r => r.Position).FirstOrDefault()?.Mention ?? "`none`", true);
        embed.AddField("Bot", user.IsBot ? "`yes`" : "`no`", true);
        embed.AddField("Pending", user.IsPending.HasValue ? (user.IsPending.Value ? "`yes`" : "`no`") : "`unknown`", true);

        if (user.AvatarUrl != null)
        {
            embed.WithThumbnail(user.AvatarUrl);
        }

        embed.WithFooter($"requested by {context.User.Username}", context.User.AvatarUrl)
            .WithTimestamp(DateTimeOffset.UtcNow);

        await context.RespondAsync(embed);
    }

    [Command("serverinfo")]
    [Aliases("guildinfo")]
    [Description("Gets information about the guild you're in")]
    [Cooldown(2, 10, CooldownBucketType.User)]
    [RequireGuild]
    public async Task GetServerInfoAsync(CommandContext context)
    {
        DiscordGuild guild = context.Guild;

        StringBuilder stringBuilder = new()
        {
            Capacity = 512
        };

        stringBuilder.AppendLine($"ID: `{guild.Id}`");
        stringBuilder.AppendLine($"Created at: `{guild.CreationTimestamp.UtcDateTime:yyyy-MM-dd HH:mm:ss}`");
        stringBuilder.AppendLine($"Owner: `{guild.Owner.Username}`");
        stringBuilder.AppendLine($"Description: `{(guild.Description.Length == 0 ? "no help provided..." : guild.Description)}`");

        if (guild.Features?.Count > 0)
        {
            stringBuilder.AppendLine($"Features: {string.Join(", ", guild.Features.Select(x => $"`{x.ToLowerInvariant()}`"))}");
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Color = EmbedUtils.EmbedColor,
            Title = guild.Name,
            Description = stringBuilder.ToString()
        };

        embedBuilder.AddField("Members", guild.MemberCount.ToString(), true);
        embedBuilder.AddField("Roles", guild.Roles.Count.ToString(), true);
        embedBuilder.AddField("Text Channels", guild.Channels.Values.Count(c => c.Type == DSharpPlus.ChannelType.Text && !c.IsPrivate).ToString(), true);
        embedBuilder.AddField("Voice Channels", guild.Channels.Values.Count(c => c.Type == DSharpPlus.ChannelType.Voice && !c.IsPrivate).ToString(), true);
        embedBuilder.AddField("Categories", guild.Channels.Values.Count(c => c.Type == DSharpPlus.ChannelType.Category && !c.IsPrivate).ToString(), true);
        embedBuilder.AddField("Roles", guild.Roles.Count.ToString(), true);
        embedBuilder.AddField("Verification Level", guild.VerificationLevel.ToString(), true);
        embedBuilder.AddField("Boosts", guild.PremiumSubscriptionCount.ToString(), true);

        embedBuilder.WithThumbnail(guild.IconUrl);

        await context.RespondAsync(embedBuilder);
    }

    [Command("ping")]
    [Aliases("botping")]
    [Description("Gets the ping of the bot only. Not yours")]
    [Cooldown(1, 20, CooldownBucketType.Global)]
    [RequireGuild]
    public async Task GetPingAsync(CommandContext context)
    {
        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = "Ping",
            Description = $"{context.Client.Ping} ms",
            Color = EmbedUtils.EmbedColor,
        };

        embedBuilder.WithThumbnail(context.Client.CurrentUser.AvatarUrl);
        embedBuilder.WithFooter("this command shows the bot's ping, not yours!", context.Client.CurrentUser.AvatarUrl);

        await context.RespondAsync(embedBuilder);
    }

    [Command("botstats")]
    [Aliases("getbotstats")]
    [Description("Gets the bot's current stats")]
    [Cooldown(1, 30, CooldownBucketType.Global)]
    [RequireGuild]
    public async Task StatsAsync(CommandContext context)
    {
        var process = Process.GetCurrentProcess();
        var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();

        var guildCount = context.Client.Guilds.Count;
        var userCount = context.Client.Guilds.Values.Sum(g => g.MemberCount);

        var memoryMB = process.PrivateMemorySize64 / (1024.0 * 1024.0);

        string description =
            $"**Uptime:** {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s\n" +
            $"**Servers:** {guildCount}\n" +
            $"**Users:** {userCount}\n" +
            $"**Memory Usage:** {memoryMB:F2} MB\n";

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = "House's Statistics",
            Description = description,
            Color = EmbedUtils.EmbedColor,
            Timestamp = DateTimeOffset.UtcNow,
        };

        embedBuilder.WithThumbnail(context.Client.CurrentUser.AvatarUrl);

        await context.RespondAsync(embed: embedBuilder);
    }

    [Command("activity")]
    [Aliases("getgameinfo", "gameinfo")]
    [Description("Shows the current game or activity of a user")]
    [Cooldown(1, 15, CooldownBucketType.User)]
    [RequireGuild]
    public async Task GameInfoAsync(CommandContext context, DiscordUser? user = null)
    {
        user ??= context.User;

        if (user is DiscordMember member)
        {
            var presence = member.Presence;

            string activityText;

            if (presence == null || presence.Activities.Count == 0)
            {
                activityText = "none";
            }
            else
            {
                var activities = presence.Activities.Select(activity =>
                {
                    string text = activity.ActivityType switch
                    {
                        ActivityType.Playing => $"playing **{activity.Name}**",
                        ActivityType.Streaming => $"streaming **{activity.Name}**",
                        ActivityType.ListeningTo => $"listening to **{activity.Name}**",
                        ActivityType.Watching => $"watching **{activity.Name}**",
                        ActivityType.Competing => $"competing in **{activity.Name}**",
                        ActivityType.Custom => activity.CustomStatus.Name ?? "custom status",
                        _ => activity.Name ?? "unknown activity"
                    };

                    if (activity.RichPresence != null)
                    {
                        if (!string.IsNullOrWhiteSpace(activity.RichPresence.Details))
                        {
                            text += $"\nDetails: {activity.RichPresence.Details}";
                        }

                        if (!string.IsNullOrWhiteSpace(activity.RichPresence.State))
                        {
                            text += $"\nState: {activity.RichPresence.State}";
                        }
                    }

                    if(!string.IsNullOrWhiteSpace(activity.StreamUrl))
                    {
                        text += $"\nStream: {activity.StreamUrl}";
                    }

                    return text;
                });

                activityText = string.Join("\n\n", activities);
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"{member.DisplayName}'s activity",
                Description = activityText,
                Color = EmbedUtils.EmbedColor
            };

            embedBuilder.WithThumbnail(member.AvatarUrl);

            await context.RespondAsync(embedBuilder);
        }
    }

    [Command("serverroles")]
    [Description("Lists roles in the server")]
    [Cooldown(1, 10, CooldownBucketType.User)]
    [RequireGuild]
    public async Task ServerRolesAsync(CommandContext context)
    {
        var guild = context.Guild;

        var roles = guild.Roles.Values.OrderByDescending(r => r.Position)
            .Select(r => $"`{r.Name} (ID: {r.Id})`");

        var description = string.Join("\n", roles.Take(50)) + "...";

        var embed = new DiscordEmbedBuilder()
            .WithTitle($"Roles in {guild.Name}")
            .WithDescription($"Showing {Math.Min(50, roles.Count())} of {roles.Count()}\n\n{description}")
            .WithColor(EmbedUtils.EmbedColor);

        await context.RespondAsync(embed);
    }
}