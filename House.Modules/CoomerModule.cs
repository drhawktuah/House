using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using House.House.Services.Gooning.Exceptions;
using House.House.Services.Gooning.HTTP;
using House.House.Utils;

namespace House.House.Modules;

[Description("House's free OnlyFans, Fansly and Patreon service for all! (THIS DOES NOT BYPASS PAYWALLS)")]
public sealed class CoomerModule : BaseCommandModule
{
    public required CoomerClient Coomer { get; set; }

    [Command("coomertest")]
    public async Task CoomerTestAsync(CommandContext context, string username)
    {
        username = username.ToLowerInvariant();

        const int batchSize = 10;

        string? foundService = null;
        List<CoomerPost> posts = [];

        foreach (var service in Coomer.Services)
        {
            try
            {
                var batch = await Coomer.GetPostsAsync(service.Name.ToLowerInvariant(), username, batchSize);
                if (batch.Count > 0)
                {
                    posts.AddRange(batch);
                    foundService = service.Name;

                    break;
                }
            }
            catch
            {
                continue;
            }
        }

        if (foundService == null || posts.Count == 0)
        {
            await context.RespondAsync($"`{username}` has no posts");
            return;
        }

        await context.RespondAsync(posts[0].Attachments[0].URL);
    }

    [Command("creator")]
    [Aliases("getcreator", "getc", "gcreator", "fetchcreator", "fetchc", "fcreator", "getfoid", "fetchfoid")]
    [Description("gets a foid- ehm, I MEAN NICE WEATHER WE'RE HAVING! 😅")]
    [Cooldown(1, 3, CooldownBucketType.User)]
    public async Task GetCreatorAsync(CommandContext context, string username)
    {
        username = username.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(username))
        {
            await context.RespondAsync("You entered an empty username");
        }

        List<string> failedServices = [];

        CoomerCreator? foundCreator = null;
        string? foundService = null;

        foreach (CoomerService service in Coomer.Services)
        {
            try
            {
                foundCreator = await Coomer.GetCreatorAsync(service.Name.ToLowerInvariant(), username);
                foundService = service.Name;

                break;
            }
            catch (CoomerCreatorNotFoundException ex)
            {
                Console.WriteLine(ex);

                failedServices.Add(service.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                failedServices.Add(service.Name);
            }
        }

        if (foundCreator == null || foundService == null)
        {
            var serviceNames = failedServices.Select(s => '`' + s + '`');

            await context.RespondAsync($"`'{username}'` could not be found on {string.Join(", ", serviceNames)}");
        }
        else
        {
            StringBuilder builder = new();

            builder.AppendLine($"**ID:** {foundCreator.ID}");
            builder.AppendLine($"**Service:** {foundService}");
            builder.AppendLine($"**Posts:** {foundCreator.FormattedPostCount}");
            builder.AppendLine($"**Indexed:** {foundCreator.Indexed:dd-MM-yyy}");
            builder.AppendLine($"**Updated:** {foundCreator.Updated:dd-MM-yyy}");

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{foundCreator.Name} -- {foundService}")
                .WithUrl(foundCreator.ProfileUrl)
                .WithDescription(builder.ToString())
                .WithColor(EmbedUtils.EmbedColor);

            if (!string.IsNullOrEmpty(foundCreator.ImageUrl))
            {
                embed.WithThumbnail(foundCreator.ImageUrl);
            }

            await context.RespondAsync(embed);
        }
    }

    [Command("posts")]
    [Aliases("getposts", "listposts", "showposts", "getfoidposts")]
    [Description("gets the content of a foid- ehm, I MEAN NICE WEATHER WE'RE HAVING! 😅")]
    [Cooldown(1, 15, CooldownBucketType.User)]
    public async Task GetPostsAsync(CommandContext context, string username)
    {
        username = username.ToLowerInvariant();

        const int batchSize = 10;

        string? foundService = null;
        List<CoomerPost> posts = [];

        foreach (var service in Coomer.Services)
        {
            try
            {
                var batch = await Coomer.GetPostsAsync(service.Name.ToLowerInvariant(), username, batchSize);
                if (batch.Count > 0)
                {
                    posts.AddRange(batch);
                    foundService = service.Name;

                    break;
                }
            }
            catch
            {
                continue;
            }
        }

        if (foundService == null || posts.Count == 0)
        {
            await context.RespondAsync($"`{username}` has no posts");
            return;
        }

        var interactivity = context.Client.GetInteractivity();

        await CoomerUtils.ShowPaginatedPostsAsync(context, Coomer, interactivity, posts, username, foundService);
    }

    [Command("getcreators")]
    [Aliases("getfoids")]
    [Description("gets foids- ehm, I MEAN NICE WEATHER WE'RE HAVING! 😅")]
    [Cooldown(1, 15, CooldownBucketType.User)]
    public async Task GetCreatorsAsync(CommandContext context)
    {
        IReadOnlyList<CoomerCreator> creators;

        try
        {
            creators = await Coomer.GetCreatorsAsync();
        }
        catch (Exception ex)
        {
            await context.RespondAsync($"Error fetching creators: {ex.Message}");
            return;
        }

        if (creators == null || creators.Count == 0)
        {
            await context.RespondAsync("No creators found.");
            return;
        }

        var interactivity = context.Client.GetInteractivity();

        await CoomerUtils.ShowPaginatedCreatorsAsync(context, interactivity, creators);
    }
}

[Description("The Coomer data module that allows complete control over saved posts, etc")]
public sealed class CoomerDataModule : BaseCommandModule
{
    public required EncryptedStorageService<UserCoomerData> StorageService { get; set; }
    public required UserDataCache UserDataCache { get; set; }

    [Command("login")]
    [Description("Securely connects you to House's 'Coomer' service")]
    [RequireDirectMessage]
    public async Task LoginAsync(CommandContext context, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            await context.RespondAsync("password cannot be empty");
            return;
        }

        if (!context.Channel.IsPrivate)
        {
            await context.Message.DeleteAsync();
            return;
        }

        var userData = await StorageService.LoadDecryptedAsync(context.User.Id, password);
        if (userData == null)
        {
            await context.RespondAsync("invalid password or no data found");
            return;
        }

        UserDataCache.CacheUserPassword(context.User.Id, password);
        UserDataCache.SetUserData(context.User.Id, userData);

        await context.RespondAsync("you are now securely logged in");
    }

    [Command("logout")]
    [Description("Securely disconnects you from House's 'Coomer' service")]
    [RequireDirectMessage]
    public async Task LogoutAsync(CommandContext context)
    {
        if (UserDataCache.TryGetUserData(context.User.Id, out var _))
        {
            UserDataCache.RemoveCachedPassword(context.User.Id);

            await context.RespondAsync("you have been securely logged out");
        }
        else
        {
            await context.RespondAsync("you have been securely logged in");
        }
    }
}