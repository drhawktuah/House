using System.Reflection;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using House.House.Attributes;
using House.House.Utils;
using Microsoft.Extensions.Logging;
using ONFQ.ONFQ.Core;
using ONFQ.ONFQ.Models;
using ONFQ.ONFQ.Utilities;

namespace House.House.Services.Fuzzy;


public sealed class HouseFuzzyMatchingService
{
    private readonly CommandsNextExtension commandsNext;
    private readonly Dictionary<string, Command> commands = [];

    private readonly bool isAdminOrOwner;

    public HouseFuzzyMatchingService(CommandsNextExtension commandsNext, bool isAdminOrOwner = false)
    {
        this.commandsNext = commandsNext;
        this.isAdminOrOwner = isAdminOrOwner;

        GetCommands();
    }

    public IEnumerable<HouseFuzzyResult> GetResults(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentNullException(nameof(query), "cannot be null, empty or whitespace");
        }

        var seen = new HashSet<Command>();

        foreach (var (name, command) in commands)
        {
            if (seen.Contains(command))
            {
                continue;
            }

            int distance = CalculateDistance(query, command);
            int percentage = CalculatePercentage(distance, Math.Max(name.Length, query.Length));

            var result = new HouseFuzzyResult
            {
                Command = command,
                ModuleName = command.Module?.ModuleType.Name ?? "no module found...",
                Distance = distance,
                Percentage = percentage,
                Similarity = $"{percentage:0.##}%",
                Icon = GetColoredPercentage(percentage)
            };

            seen.Add(command);

            yield return result;
        }
    }

    public Task<DiscordEmbed> ToDiscordEmbed(string query, IEnumerable<HouseFuzzyResult> results)
    {
        var groupedResults = results
            .Where(r => r.Percentage >= 0)
            .GroupBy(r => r.Percentage)
            .OrderByDescending(g => g.Key)
            .ToList();

        var embedBuilder = new DiscordEmbedBuilder()
            .WithColor(EmbedUtils.EmbedColor)
            .WithFooter("emojis indicate how closely a command matched your query", commandsNext.Client.CurrentUser.AvatarUrl)
            .WithThumbnail(commandsNext.Client.CurrentUser.GetAvatarUrl(DSharpPlus.ImageFormat.Png, 1024));

        var description = new StringBuilder();

        if (groupedResults.Count == 0)
        {
            description.AppendLine($"no matches for '{query}' have been found");
        }
        else
        {
            embedBuilder.Title = $"command '{query}' cannot be found. showing alternatives:";

            foreach (var group in groupedResults)
            {
                string icon = GetColoredPercentage(group.Key);
                string similarity = $"`{group.Key}%`".PadLeft(5);
                string commandList = string.Join(", ", group.Select(r => $"`{r.Command.Name}`"));

                description.AppendLine($"{icon} {similarity} {commandList}");
            }
        }

        embedBuilder.WithDescription(description.ToString());
        return Task.FromResult(embedBuilder.Build());
    }

    private static int CalculateDistance(string query, Command command)
    {
        // If strings are equal, distance is zero
        if (query == command.Name)
        {
            return 0;
        }

        int[,] distances = new int[query.Length + 1, command.Name.Length + 1];

        for (int i = 0; i <= query.Length; i++)
        {
            distances[i, 0] = i;
        }

        for (int j = 0; j <= command.Name.Length; j++)
        {
            distances[0, j] = j;
        }

        for (int k = 1; k <= query.Length; k++)
        {
            for (int l = 1; l <= command.Name.Length; l++)
            {
                int cost = (query[k - 1] == command.Name[l - 1]) ? 0 : 1;

                int insertion = distances[k, l - 1] + 1;
                int deletion = distances[k - 1, l] + 1;
                int substitution = distances[k - 1, l - 1] + cost;

                int distance = Math.Min(Math.Min(insertion, deletion), substitution);

                if (k > 1 && l > 1 && query[k - 1] == command.Name[l - 2] && query[k - 2] == command.Name[l - 1])
                {
                    int transposition = distances[k - 2, l - 2] + cost;
                    distance = Math.Min(distance, transposition);
                }

                distances[k, l] = distance;
            }
        }

        return distances[query.Length, command.Name.Length];
    }

    private static int CalculatePercentage(int distance, int maxLength)
    {
        return (int)((1.0 - ((double)distance / maxLength)) * 100);
    }

    private static string GetColoredPercentage(double similarity)
    {
        if (similarity == 0.0)
        {
            return "⚫";
        }

        if (similarity <= 10.0)
        {
            return "🔴";
        }

        if (similarity <= 20.0)
        {
            return "🟠";
        }

        if (similarity <= 35.0)
        {
            return "🟡";
        }

        if (similarity <= 50.0)
        {
            return "🟢";
        }

        if (similarity <= 65.0)
        {
            return "🔵";
        }

        if (similarity <= 80.0)
        {
            return "🟣";
        }

        if (similarity <= 95.0)
        {
            return "⚪";
        }

        return "🟤";
    }

    private void GetCommands()
    {
        commands.Clear();

        foreach (var (name, command) in commandsNext.RegisteredCommands)
        {
            if (command.IsHidden && !isAdminOrOwner)
            {
                continue;
            }

            if (!commands.ContainsKey(name))
            {
                commands[name] = command;
            }
        }

        Console.WriteLine($"Loaded {commands.Count} commands into matcher");
    }
}


/*
public sealed class HouseFuzzyMatchingService
{
    private readonly CommandsNextExtension commandsNext;

    private readonly SpectrumFinder<Command> spectrumFinder;
    private readonly Spectrum<Command> commands;

    private const int MaxLength = 50;

    public HouseFuzzyMatchingService(CommandsNextExtension commandsNext)
    {
        this.commandsNext = commandsNext;

        commands = new(MaxLength, commandsNext.RegisteredCommands.Count);
        spectrumFinder = new(commands);

        BuildSpectrum();
    }

    public IEnumerable<HouseFuzzyResult> GetResults(string query, int maxResults = 10, float threshold = 0.55f, BlendMode mode = BlendMode.TranspositionSIMD)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentNullException(nameof(query), "Query cannot be null, empty or whitespace");
        }

        ReadOnlySpan<float> queryVector = CharUtils.BuildQueryVector(query, MaxLength);

        var results = spectrumFinder.FindMatches(queryVector, maxResults, threshold, mode);

        foreach (var result in results)
        {
            yield return new HouseFuzzyResult
            {
                Command = result.Item,
                ModuleName = result.Item.Module?.ModuleType.Name ?? "no module found...",
                Percentage = (int)(result.Similarity * 100),
                Similarity = $"{result.Similarity * 100:0.##}%",
                Icon = GetColoredPercentage(result.Similarity * 100)
            };
        }
    }

    public Task<DiscordEmbed> ToDiscordEmbed(string query, IEnumerable<HouseFuzzyResult> results)
    {
        var groupedResults = results
            .Where(r => r.Percentage >= 0)
            .GroupBy(r => r.Percentage)
            .OrderByDescending(g => g.Key)
            .ToList();

        var embedBuilder = new DiscordEmbedBuilder()
            .WithColor(EmbedUtils.EmbedColor)
            .WithFooter("emojis indicate how closely a command matched your query", commandsNext.Client.CurrentUser.AvatarUrl)
            .WithThumbnail(commandsNext.Client.CurrentUser.GetAvatarUrl(DSharpPlus.ImageFormat.Png, 1024));

        var description = new StringBuilder();

        if (groupedResults.Count == 0)
        {
            description.AppendLine($"no matches for '{query}' have been found");
        }
        else
        {
            embedBuilder.Title = $"command '{query}' cannot be found. showing alternatives:";

            foreach (var group in groupedResults)
            {
                string icon = GetColoredPercentage(group.Key);
                string similarity = $"`{group.Key}%`".PadLeft(5);
                string commandList = string.Join(", ", group.Select(r => $"`{r.Command.Name}`"));

                description.AppendLine($"{icon} {similarity} {commandList}");
            }
        }

        embedBuilder.WithDescription(description.ToString());
        return Task.FromResult(embedBuilder.Build());
    }

    private static string GetColoredPercentage(double similarity)
    {
        return similarity switch
        {
            <= 0 => "⚫",
            <= 10 => "🔴",
            <= 20 => "🟠",
            <= 35 => "🟡",
            <= 50 => "🟢",
            <= 65 => "🔵",
            <= 80 => "🟣",
            <= 95 => "⚪",
            _ => "🟤"
        };
    }

    public void BuildSpectrum()
    {
        foreach (Command command in commandsNext.RegisteredCommands.Values)
        {
            if (commands.Contains(command))
            {
                continue;
            }

            if(command.Module?.ModuleType.GetCustomAttribute<HiddenAttribute>() is not null)
            {
                continue;
            }

            ReadOnlySpan<float> commandVector = CharUtils.BuildQueryVector(command.Name, MaxLength);
            TraverseCommands(command, commandVector);

            commandsNext.Client.Logger.LogInformation("Added '{CommandName}' to matching service", command.Name);
        }
    }

    private void TraverseCommands(Command command, ReadOnlySpan<float> vector)
    {
        commands.Add(command, vector);

        if (command is CommandGroup group && group.Children is { Count: > 0 })
        {
            foreach (var child in group.Children)
            {
                ReadOnlySpan<float> childVector = CharUtils.BuildQueryVector(command.Name, MaxLength);
                TraverseCommands(child, childVector);
            }
        }
    }
}
*/