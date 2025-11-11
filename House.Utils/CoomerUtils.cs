using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using House.House.Services.Gooning.HTTP;

namespace House.House.Utils;

public static class CoomerUtils
{
    public static string FormatPostDescription(CoomerPost post)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"**ID:** {post.ID}");
        builder.AppendLine($"**Date:** {post.Published:dd-MM-yyyy}");
        builder.AppendLine($"**Files:** {post.Attachments.Count}");
        builder.AppendLine($"**Text:** {(string.IsNullOrWhiteSpace(post.Text) ? "*None*" : Truncate(post.Text, 200))}");

        return builder.ToString();
    }

    public static DiscordEmbedBuilder BuildCoomerPostEmbed(CoomerPost post, int index, int total, string creatorName, string service)
    {
        DiscordEmbedBuilder embedBuilder = new();

        embedBuilder.WithTitle($"{creatorName} - [{service}]");
        embedBuilder.WithColor(EmbedUtils.EmbedColor);
        embedBuilder.WithDescription(FormatPostDescription(post));
        embedBuilder.WithFooter($"Post ID: {post.ID}\nCreated: {post.Published:dd MMM yyyy}");

        return embedBuilder;
    }

    public static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value[..maxLength] + "...";
    }

    public static string FormatServiceList(IEnumerable<string> services)
    {
        return string.Join(", ", services.Select(s => $"`{s}`"));
    }

    public static async Task CachePostEmbedsAsync(List<CoomerPost> allPosts, Dictionary<int, List<DiscordEmbedBuilder>> embedCache, string username, string service, int startIndex = 0)
    {
        for (int i = startIndex; i < allPosts.Count; i++)
        {
            if (embedCache.ContainsKey(i))
            {
                continue;
            }

            var post = allPosts[i];
            var embeds = new List<DiscordEmbedBuilder>();

            if (post.Attachments.Count == 0)
            {
                var embed = BuildCoomerPostEmbed(post, i + 1, allPosts.Count, username, service);

                embeds.Add(embed);
            }
            else
            {
                for (int j = 0; j < post.Attachments.Count; j++)
                {
                    var file = post.Attachments[j];
                    var embed = BuildCoomerPostEmbed(post, i + 1, allPosts.Count, username, service)
                        .WithImageUrl(file.URL)
                        .WithFooter($"Post ID: {post.ID}\nAttachment {j + 1} of {post.Attachments.Count}\nCreated: {post.Published:dd MMM yyyy}");

                    embeds.Add(embed);
                }
            }

            embedCache[i] = embeds;
        }

        await Task.Yield();
    }

    public static async Task ShowPaginatedPostsAsync(CommandContext context, CoomerClient client, InteractivityExtension interactivity, List<CoomerPost> allPosts, string username, string service)
    {
        int currentIndex = 0;
        int currentAttachmentIndex = 0;

        bool stopped = false;

        var embedCache = new Dictionary<int, List<DiscordEmbedBuilder>>();

        await CachePostEmbedsAsync(allPosts, embedCache, username, service);

        var postNavigationRow = new List<DiscordComponent>
        {
            new DiscordButtonComponent(ButtonStyle.Primary, "start", "⏮️"),
            new DiscordButtonComponent(ButtonStyle.Primary, "prev", "⏪"),
            new DiscordButtonComponent(ButtonStyle.Danger, "stop", "⏹️"),
            new DiscordButtonComponent(ButtonStyle.Primary, "next", "⏩"),
            new DiscordButtonComponent(ButtonStyle.Primary, "fastforward", "⏭️"),
        };

            var attachmentNavigationRow = new[]
            {
            new DiscordButtonComponent(ButtonStyle.Secondary, "prevfile", "⬅️"),
            new DiscordButtonComponent(ButtonStyle.Secondary, "nextfile", "➡️"),
        };

        var message = await context.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .WithEmbed(embedCache[currentIndex][currentAttachmentIndex])
            .AddComponents(postNavigationRow)
            .AddComponents(attachmentNavigationRow));

        while (!stopped)
        {
            var result = await interactivity.WaitForButtonAsync(message, context.User, TimeSpan.FromMinutes(2));

            if (result.TimedOut)
            {
                await message.DeleteAsync();
                break;
            }

            var id = result.Result.Id;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            var currentPost = allPosts[currentIndex];
            var files = currentPost.Attachments;

            if (id == "start")
            {
                currentIndex = 0;
                currentAttachmentIndex = 0;
            }
            else if (id == "prev")
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                    currentAttachmentIndex = 0;
                }
            }
            else if (id == "next")
            {
                if (currentIndex < allPosts.Count - 1)
                {
                    currentIndex++;
                    currentAttachmentIndex = 0;
                }
                else
                {
                    int newLimit = allPosts.Count + 10;
                    var updatedPosts = await client.GetPostsAsync(service, username, newLimit);
                    if (updatedPosts.Count > allPosts.Count)
                    {
                        allPosts = updatedPosts.ToList();

                        await CachePostEmbedsAsync(allPosts, embedCache, username, service, startIndex: embedCache.Count);

                        currentIndex++;
                        currentAttachmentIndex = 0;
                    }
                }
            }
            else if (id == "fastforward")
            {
                int newLimit = allPosts.Count + 50;
                var updatedPosts = await client.GetPostsAsync(service, username, newLimit);
                if (updatedPosts.Count > allPosts.Count)
                {
                    allPosts = updatedPosts.ToList();

                    await CachePostEmbedsAsync(allPosts, embedCache, username, service, startIndex: embedCache.Count);
                }

                currentIndex = allPosts.Count - 1;
                currentAttachmentIndex = 0;
            }
            else if (id == "prevfile")
            {
                if (files.Count > 0)
                {
                    currentAttachmentIndex = (currentAttachmentIndex - 1 + files.Count) % files.Count;
                }
            }
            else if (id == "nextfile")
            {
                if (files.Count > 0)
                {
                    currentAttachmentIndex = (currentAttachmentIndex + 1) % files.Count;
                }
            }
            else if (id == "stop")
            {
                stopped = true;
                await message.DeleteAsync();
                break;
            }

            var embedsForPost = embedCache[currentIndex];
            var embedToShow = embedsForPost.Count > 0
                ? embedsForPost[Math.Min(currentAttachmentIndex, embedsForPost.Count - 1)]
                : BuildCoomerPostEmbed(allPosts[currentIndex], currentIndex + 1, allPosts.Count, username, service);

            var newMessageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embedToShow)
                .AddComponents(postNavigationRow)
                .AddComponents(attachmentNavigationRow);

            await message.ModifyAsync(newMessageBuilder);
        }

        if (stopped)
        {
            await message.DeleteAsync();
        }
    }

    public static async Task ShowPaginatedCreatorsAsync(CommandContext context, InteractivityExtension interactivity, IReadOnlyList<CoomerCreator> creators, int creatorsPerPage = 3)
    {
        if (creators.Count == 0)
        {
            await context.Channel.SendMessageAsync("No creators found");
            return;
        }

        int currentPage = 0;
        int totalPages = (int)Math.Ceiling(creators.Count / (double)creatorsPerPage);

        List<DiscordEmbed> BuildPageEmbeds(int page)
        {
            var embeds = new List<DiscordEmbed>();

            int startIndex = page * creatorsPerPage;
            int endIndex = Math.Min(startIndex + creatorsPerPage, creators.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                var creator = creators[i];

                var embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle($"{creator.Name} — ({creator.Service})")
                    .WithUrl(creator.ProfileUrl)
                    .WithThumbnail(creator.ImageUrl)
                    .AddField("Posts", creator.PostCount.ToString(), true)
                    .AddField("DMs", creator.DMCount.ToString(), true)
                    .AddField("Shares", creator.ShareCount.ToString(), true)
                    .AddField("Chats", creator.ChatCount.ToString(), true)
                    .AddField("Indexed", creator.Indexed.ToString("yyyy-MM-dd HH:mm UTC"), true)
                    .AddField("Updated", creator.Updated.ToString("yyyy-MM-dd HH:mm UTC"), true)
                    .WithFooter($"Creator {i + 1} of {creators.Count}")
                    .WithTimestamp(DateTime.UtcNow)
                    .WithColor(EmbedUtils.EmbedColor);

                embeds.Add(embedBuilder.Build());
            }

            return embeds;
        }

        var embedsToShow = BuildPageEmbeds(currentPage);

        var navButtons = new List<DiscordComponent>
        {
            new DiscordButtonComponent(ButtonStyle.Primary, "prevPage", "⬅️ Previous"),
            new DiscordButtonComponent(ButtonStyle.Danger, "stop", "⏹️ Stop"),
            new DiscordButtonComponent(ButtonStyle.Primary, "nextPage", "Next ➡️"),
        };

        var message = await context.Channel.SendMessageAsync(new DiscordMessageBuilder()
            .AddEmbeds(embedsToShow)
            .AddComponents(navButtons));

        bool stopped = false;

        while (!stopped)
        {
            var interactionResult = await interactivity.WaitForButtonAsync(message, context.User, TimeSpan.FromMinutes(2));
            if (interactionResult.TimedOut)
            {
                await message.DeleteAsync();
                break;
            }

            var id = interactionResult.Result.Id;
            await interactionResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            if (id == "prevPage")
            {
                currentPage = (currentPage == 0) ? totalPages - 1 : currentPage - 1;
            }
            else if (id == "nextPage")
            {
                currentPage = (currentPage + 1) % totalPages;
            }
            else if (id == "stop")
            {
                stopped = true;

                await message.DeleteAsync();
                break;
            }

            embedsToShow = BuildPageEmbeds(currentPage);

            var newMsgBuilder = new DiscordMessageBuilder()
                .AddEmbeds(embedsToShow)
                .AddComponents(navButtons);

            await message.ModifyAsync(newMsgBuilder);
        }
    }

    /*
    public static async Task ShowPaginatedPostsAsync(CommandContext context, CoomerClient client, InteractivityExtension interactivity, List<CoomerPost> allPosts, string username, string service)
    {
        int currentIndex = 0;
        int currentAttachmentIndex = 0;

        bool stopped = false;

        var postNavigationRow = new List<DiscordComponent>
        {
            new DiscordButtonComponent(ButtonStyle.Primary, "start", "⏮️"),       // Go to first post
            new DiscordButtonComponent(ButtonStyle.Primary, "prev", "⏪"),        // Previous post
            new DiscordButtonComponent(ButtonStyle.Danger, "stop", "⏹️"),         // Stop navigation
            new DiscordButtonComponent(ButtonStyle.Primary, "next", "⏩"),        // Next post
            new DiscordButtonComponent(ButtonStyle.Primary, "fastforward", "⏭️"), // Jump to last
        };

        var attachmentNavigationRow = new[]
        {
            new DiscordButtonComponent(ButtonStyle.Secondary, "prevfile", "⬅️"),
            new DiscordButtonComponent(ButtonStyle.Secondary, "nextfile", "➡️"),
        };

        var currentPost = allPosts[currentIndex];
        var embedBuilder = BuildCoomerPostEmbed(currentPost, currentIndex + 1, allPosts.Count, username, service);

        if (currentPost.Attachments.Count > 0)
        {
            var currentFile = currentPost.Attachments[currentAttachmentIndex];

            embedBuilder.WithImageUrl(currentFile.URL);
            embedBuilder.WithFooter($"Post ID: {currentPost.ID}\nAttachment {currentAttachmentIndex + 1} of {currentPost.Attachments.Count}\nCreated: {currentPost.Published:dd MMM yyyy}");
        }

        var builder = new DiscordMessageBuilder()
            .WithEmbed(embedBuilder)
            .AddComponents(postNavigationRow)
            .AddComponents(attachmentNavigationRow);

        var message = await context.Channel.SendMessageAsync(builder);

        while (!stopped)
        {
            var result = await interactivity.WaitForButtonAsync(message, context.User, TimeSpan.FromMinutes(2));

            if (result.TimedOut)
            {
                await message.DeleteAsync();
                break;
            }

            var id = result.Result.Id;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            currentPost = allPosts[currentIndex];
            var files = currentPost.Attachments;

            if (id == "start")
            {
                currentIndex = 0;
                currentAttachmentIndex = 0;
            }
            else if (id == "prev")
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                    currentAttachmentIndex = 0;
                }
            }
            else if (id == "skip5")
            {
                currentIndex = Math.Min(allPosts.Count - 1, currentIndex + 5);
                currentAttachmentIndex = 0;
            }
            else if (id == "next")
            {
                if (currentIndex < allPosts.Count - 1)
                {
                    currentIndex++;
                    currentAttachmentIndex = 0;
                }
                else
                {
                    int newLimit = allPosts.Count + 10;
                    var updatedPosts = await client.GetPostsAsync(service, username, newLimit);
                    if (updatedPosts.Count > allPosts.Count)
                    {
                        allPosts = updatedPosts.ToList();
                        currentIndex++;
                        currentAttachmentIndex = 0;
                    }
                }
            }
            else if (id == "fastforward")
            {
                int newLimit = allPosts.Count + 50;
                var updatedPosts = await client.GetPostsAsync(service, username, newLimit);
                if (updatedPosts.Count > allPosts.Count)
                {
                    allPosts = updatedPosts.ToList();
                }

                currentIndex = allPosts.Count - 1;
                currentAttachmentIndex = 0;
            }
            else if (id == "prevfile")
            {
                if (files.Count > 0)
                {
                    currentAttachmentIndex = (currentAttachmentIndex - 1 + files.Count) % files.Count;
                }
            }
            else if (id == "nextfile")
            {
                if (files.Count > 0)
                {
                    currentAttachmentIndex = (currentAttachmentIndex + 1) % files.Count;
                }
            }
            else if (id == "stop")
            {
                stopped = true;

                await message.DeleteAsync();
                break;
            }

            currentPost = allPosts[currentIndex];
            files = currentPost.Attachments;

            embedBuilder = BuildCoomerPostEmbed(currentPost, currentIndex + 1, allPosts.Count, username, service);

            if (stopped == true)
            {
                await message.DeleteAsync();

                break;
            }

            if (files.Count > 0)
            {
                var currentFile = files[currentAttachmentIndex];

                embedBuilder.WithImageUrl(currentFile.URL);
                embedBuilder.WithFooter($"Post ID: {currentPost.ID}\nAttachment {currentAttachmentIndex + 1} of {files.Count}\nCreated: {currentPost.Published:dd MMM yyyy}");
            }

            var newMessageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embedBuilder)
                .AddComponents(postNavigationRow)
                .AddComponents(attachmentNavigationRow);

            await message.ModifyAsync(newMessageBuilder);
        }
    }
    */

    /*
    public static async Task ShowPaginatedPostsAsync(CommandContext context, CoomerClient client, InteractivityExtension interactivity, List<CoomerPost> allPosts, string username, string service)
    {
        bool stopped = false;

        int currentIndex = 0;
        int currentAttachmentIndex = 0;

        string[] emojis = ["⏮️", "⬅️", "🔙", "🔁", "🔂", "🔜", "➡️", "⏭️", "⏹️"];

        var message = await context.Channel.SendMessageAsync(BuildCoomerPostEmbed(allPosts[currentIndex], currentIndex + 1, allPosts.Count, username, service));

        foreach (var emoji in emojis)
        {
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode(emoji));
        }

        while (!stopped)
        {
            var reaction = await interactivity.WaitForReactionAsync(x => x.Message == message && x.User == context.User, TimeSpan.FromMinutes(2));

            if (reaction.TimedOut)
            {
                await message.DeleteAsync();
                break;
            }

            string emoji = reaction.Result.Emoji.GetDiscordName();

            var currentPost = allPosts[currentIndex];
            var files = currentPost.Attachments;

            if (emoji == emojis[0])
            {
                currentIndex = 0;
                currentAttachmentIndex = 0;
            }
            else if (emoji == emojis[1])
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                    currentAttachmentIndex = 0;
                }
            }
            else if (emoji == emojis[2])
            {
                currentIndex = Math.Min(allPosts.Count, currentIndex + 5);
                currentAttachmentIndex = 0;
            }
            else if (emoji == emojis[3])
            {
                if (currentIndex < allPosts.Count - 1)
                {
                    currentIndex++;
                    currentAttachmentIndex = 0;
                }
                else
                {
                    int newLimit = allPosts.Count + 10;

                    var updatedPosts = await client.GetPostsAsync(service, username, newLimit);
                    if (updatedPosts.Count > allPosts.Count)
                    {
                        allPosts = updatedPosts.ToList();

                        currentIndex++;
                        currentAttachmentIndex = 0;
                    }
                }
            }
            else if (emoji == emojis[4])
            {
                int newLimit = allPosts.Count + 50;

                var updatedPosts = await client.GetPostsAsync(service, username, newLimit);
                if (updatedPosts.Count > allPosts.Count)
                {
                    allPosts = updatedPosts.ToList();
                }

                currentIndex = allPosts.Count - 1;
                currentAttachmentIndex = 0;
            }
            else if (emoji == emojis[5])
            {
                if (files.Count > 0)
                {
                    currentAttachmentIndex = (currentAttachmentIndex - 1 + files.Count) % files.Count;
                }
            }
            else if (emoji == emojis[6])
            {
                if (files.Count > 0)
                {
                    currentAttachmentIndex = (currentAttachmentIndex + 1) % files.Count;
                }
            }
            else if (emoji == emojis[7])
            {
                await message.DeleteAsync();
                stopped = true;

                break;
            }

            currentPost = allPosts[currentIndex];
            files = currentPost.Attachments.ToList();

            var embedBuilder = BuildCoomerPostEmbed(currentPost, currentIndex + 1, allPosts.Count, username, service);

            if (files.Count > 0)
            {
                var currentFile = files[currentAttachmentIndex];

                embedBuilder.WithImageUrl(currentFile.URL);
                embedBuilder.WithFooter($"Post ID: {currentPost.ID}\nAttachment {currentAttachmentIndex + 1} of {files.Count}\nCreated: {currentPost.Published:dd MMM yyyy}");
            }

            await message.ModifyAsync(embedBuilder.Build());
            await message.DeleteReactionAsync(reaction.Result.Emoji, context.User);
        }
    }
    */
}