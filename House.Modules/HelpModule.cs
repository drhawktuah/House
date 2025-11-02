using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using House.House.Core;
using House.House.Services.Fuzzy;

namespace House.House.Modules;

/*
public async Task HelpAsync(CommandContext context, [RemainingText] string? name = null)
{
    var commandsNext = context.CommandsNext;
    var helpFormatter = new HateHelpFormatter(context);

    if (string.IsNullOrWhiteSpace(name))
    {
        var topLevelCommands = commandsNext.RegisteredCommands.Values.Where(c => c.Parent is null);
        helpFormatter.WithSubcommands(topLevelCommands);

        var interactivity = context.Client.GetInteractivity();
        TimeSpan timeout = TimeSpan.FromSeconds(30);

        await interactivity.SendPaginatedMessageAsync(
            context.Channel,
            context.User,
            helpFormatter.Pages,
            timeout,
            PaginationBehaviour.Ignore,
            ButtonPaginationBehavior.DeleteMessage
        );
    }
    else
    {
        var command = commandsNext.FindCommand(name, out _);

        if (command is not null)
        {
            helpFormatter.WithCommand(command);

            var interactivity = context.Client.GetInteractivity();
            TimeSpan timeout = TimeSpan.FromSeconds(30);

            await interactivity.SendPaginatedMessageAsync(
                context.Channel,
                context.User,
                helpFormatter.Pages,
                timeout,
                PaginationBehaviour.Ignore,
                ButtonPaginationBehavior.DeleteMessage
            );
        }
        else
        {
            var results = FuzzyMatchingService.GetResults(name).ToList();

            if (results.Count == 0)
            {
                await context.RespondAsync($"No command named `{name}` was found, and no similar commands could be suggested.");
                return;
            }

            var embed = await FuzzyMatchingService.ToDiscordEmbed(name, results);
            await context.RespondAsync(embed: embed);
        }
    }
}
*/

/*
[Description("House's help module")]
public sealed class HelpModule : BaseCommandModule
{
    public required CommandsNextExtension CommandsNext { get; set; }
    public required InteractivityExtension Interactivity { get; set; }
    public required HateFuzzyMatchingService FuzzyMatchingService { get; set; }

    private readonly TimeSpan timeout = TimeSpan.FromMinutes(2);

    [Command("help")]
    [Description("What'd you think this does?")]
    public async Task HelpAsync(CommandContext context, [RemainingText] string? query = null)
    {
        HateHelpFormatter helpFormatter = new(context);

        if (string.IsNullOrWhiteSpace(query))
        {
            IEnumerable<Command> totalCommands = CommandsNext.RegisteredCommands.Values.Where(x => !x.IsHidden);
            helpFormatter.WithSubcommands(totalCommands);

            await Interactivity.SendPaginatedMessageAsync(
                context.Channel,
                context.User,
                helpFormatter.Pages,
                timeout,
                PaginationBehaviour.Ignore,
                ButtonPaginationBehavior.DeleteMessage
            );
        }
        else
        {
            var command = CommandsNext.FindCommand(query, out _);

            if (command is not null)
            {
                helpFormatter.WithCommand(command);

                await Interactivity.SendPaginatedMessageAsync(
                    context.Channel,
                    context.User,
                    helpFormatter.Pages,
                    timeout,
                    PaginationBehaviour.Ignore,
                    ButtonPaginationBehavior.DeleteMessage
                );
            }
            else
            {
                var results = FuzzyMatchingService.GetResults(query);
                if (!results.Any())
                {
                    await context.RespondAsync($"no command named `'{query}'` was found, and no similar commands could be suggested");
                    return;
                }

                var embed = await FuzzyMatchingService.ToDiscordEmbed(query, results);
                await context.RespondAsync(embed);
            }
        }
    }
}
*/

[Description("House's help module")]
public sealed class HelpModule : BaseCommandModule
{
    public required CommandsNextExtension CommandsNext { get; set; }
    public required InteractivityExtension Interactivity { get; set; }
    public required HouseFuzzyMatchingService FuzzyMatchingService { get; set; }

    private readonly TimeSpan timeout = TimeSpan.FromMinutes(2);

    [Command("help")]
    [Description("What'd you think this does?")]
    public async Task HelpAsync(CommandContext context, [RemainingText] string? query = null)
    {
        var helpFormatter = new HouseHelpFormatter(context);

        if (string.IsNullOrWhiteSpace(query))
        {
            var visibleCommands = CommandsNext.RegisteredCommands.Values.Where(cmd => !cmd.IsHidden);

            helpFormatter.WithSubcommands(visibleCommands);
            await SendStackedHelpAsync(context, helpFormatter.Pages);
            return;
        }

        var matchedCommand = CommandsNext.FindCommand(query, out _);
        if (matchedCommand is not null)
        {
            helpFormatter.WithCommand(matchedCommand);

            await SendStackedHelpAsync(context, helpFormatter.Pages);
            return;
        }

        var suggestions = FuzzyMatchingService.GetResults(query);
        if (suggestions.Any())
        {
            var embed = await FuzzyMatchingService.ToDiscordEmbed(query, suggestions);
            await context.RespondAsync(embed);
        }
        else
        {
            await context.RespondAsync(
                $"No command named `'{query}'` was found, and no similar commands could be suggested");
        }
    }

    private async Task SendPaginatedHelpAsync(CommandContext context, IReadOnlyList<Page> pages)
    {
        await Interactivity.SendPaginatedMessageAsync(
            context.Channel,
            context.User,
            pages,
            timeout,
            PaginationBehaviour.Ignore,
            ButtonPaginationBehavior.DeleteMessage
        );
    }

    private async Task SendStackedHelpAsync(CommandContext context, IReadOnlyList<Page> pages)
    {
        if (pages.Count == 0)
        {
            return;
        }

        const int EmbedsPerMessage = 3;

        int totalMessages = (pages.Count + EmbedsPerMessage - 1) / EmbedsPerMessage;

        for (int i = 0; i < totalMessages; i++)
        {
            DiscordMessageBuilder messageBuilder = new();

            for (int j = 0; i < EmbedsPerMessage; i++)
            {
                int pageIndex = i * EmbedsPerMessage + j;
                if (pageIndex >= pages.Count)
                {
                    break;
                }

                var page = pages[i];
                var embed = page.Embed;
                if (embed != null)
                {
                    messageBuilder.AddEmbed(embed);
                }
            }
        }
    }
}