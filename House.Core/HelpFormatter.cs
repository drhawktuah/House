using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using House.House.Utils;
using MongoDB.Driver.Linq;

namespace House.House.Core;


[Obsolete("Obsolete help formatter")]
public sealed partial class OldHouseFormatter : BaseHelpFormatter
{
    public IReadOnlyList<Page> Pages => pages;

    private readonly CommandContext context;
    private Command? command;

    private readonly List<Page> pages = [];

    private DiscordEmbedBuilder embedBuilder = new()
    {
        Color = EmbedUtils.EmbedColor
    };

    public OldHouseFormatter(CommandContext context) : base(context)
    {
        this.context = context;
    }

    public override BaseHelpFormatter WithCommand(Command command)
    {
        this.command = command;

        FormatSingularCommand();
        FormatSingularDescription();
        FormatAliases();
        FormatSingularOverloads();

        AddCurrentPage();

        return this;
    }

    public BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands, int pageNumber = 1, int totalPages = 1)
    {
        var commandsByModule = subcommands
        .Where(c => c.Module is not null)
        .GroupBy(c => c.Module?.ModuleType.Name);

        foreach (var group in commandsByModule)
        {
            embedBuilder.WithTitle($"{group.Key} (Page {pageNumber}/{totalPages})");

            StringBuilder stringBuilder = new(4096);

            foreach (var command in group.DistinctBy(c => c.Name))
            {
                string description = string.IsNullOrWhiteSpace(command.Description) ? "`no help provided...`" : $"`{command.Description}`";

                stringBuilder.AppendLine($"`{context.Prefix}{command.Name}` -- {description}");
            }

            embedBuilder.WithDescription(stringBuilder.ToString().Trim());
            embedBuilder.WithFooter($"Page {pageNumber} of {totalPages}", context.Client.CurrentUser.AvatarUrl);

            AddCurrentPage();
        }

        return this;
    }

    public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
    {
        var commandsByModule = subcommands
            .Where(c => c.Module is not null)
            .GroupBy(c => c.Module?.ModuleType.Name);

        foreach (var group in commandsByModule)
        {
            embedBuilder.WithTitle(group.Key);

            StringBuilder stringBuilder = new(4096);

            foreach (var command in group.DistinctBy(c => c.Name))
            {
                string description = string.IsNullOrWhiteSpace(command.Description) ? "`no help provided...`" : $"`{command.Description}`";

                stringBuilder.AppendLine($"`{context.Prefix}{command.Name}` -- {description}");
            }

            embedBuilder.WithDescription(stringBuilder.ToString().Trim());

            AddCurrentPage();
        }

        return this;
    }

    public override CommandHelpMessage Build()
    {
        return new();
    }
}

public sealed partial class OldHouseFormatter : BaseHelpFormatter
{
    private void FormatSingularCommand()
    {
        embedBuilder.Title = command?.QualifiedName;
    }

    private void FormatSingularDescription()
    {
        StringBuilder stringBuilder = new()
        {
            Length = 1024
        };

        stringBuilder.AppendLine(command?.Description is null || command.Description.Length == 0 ? command?.Description : "`no help found...`");

        embedBuilder.Description = stringBuilder.ToString();
    }

    private void FormatSingularOverloads()
    {
        StringBuilder stringBuilder = new()
        {
            Length = 512
        };

        if (command?.Overloads.Count > 0)
        {
            foreach (CommandOverload? overload in command.Overloads.OrderByDescending(x => x.Priority))
            {
                string arguments;

                if (overload.Arguments.Count > 0)
                {
                    arguments = string.Join(' ', overload.Arguments.Select(a => $"`{a.Name} [{a.Type}]`"));

                    stringBuilder.AppendLine($"`{context.Prefix}{command.QualifiedName}` {arguments}");
                }
                else
                {
                    stringBuilder.AppendLine($"`{context.Prefix}{command.QualifiedName}`");
                }
            }

            embedBuilder.AddField("arguments", stringBuilder.ToString().Trim(), false);
        }
    }

    private void FormatAliases()
    {
        embedBuilder.AddField("aliases", command?.Aliases.Count > 0 ? string.Join(", ", command.Aliases.OrderByDescending(x => $"`{x}`")) : "`no aliases found...`", false);
    }

    private void AddCurrentPage()
    {
        embedBuilder.WithThumbnail(context.Client.CurrentUser.AvatarUrl, 256, 256);

        var page = new Page(embed: embedBuilder);
        pages.Add(page);

        embedBuilder = new()
        {
            Color = EmbedUtils.EmbedColor
        };
    }
}

public sealed class HouseHelpFormatter : BaseHelpFormatter
{
    public IReadOnlyList<Page> Pages => pages;
    private readonly List<Page> pages = [];

    private readonly Command? command;

    private readonly CommandContext context;

    private DiscordEmbedBuilder embedBuilder = new()
    {
        Color = EmbedUtils.EmbedColor
    };

    public HouseHelpFormatter(CommandContext context) : base(context)
    {
        this.context = context;

        command = context.Command;
    }

    public override BaseHelpFormatter WithCommand(Command command)
    {
        var aliases = HouseHelpFormatterUtils.GetCommandAliases(command);
        var description = HouseHelpFormatterUtils.GetCommandDescription(command);
        var overloads = HouseHelpFormatterUtils.GetCommandOverloads(command, context);

        embedBuilder.Title = command.QualifiedName;
        embedBuilder.Description = description.ToString();

        embedBuilder.AddField("overloads", overloads.ToString());
        embedBuilder.AddField("aliases", aliases.ToString());

        Page page = HouseHelpFormatterUtils.BuildPage(embedBuilder);

        pages.Add(page);

        return this;
    }

    public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
    {
        var commandModules = subcommands
            .Where(c => c.Module is not null)
            .GroupBy(c => c.Module?.ModuleType.Name);

        foreach (var group in commandModules)
        {
            embedBuilder.Title = group.Key;

            StringBuilder commandBuilder = new()
            {
                Capacity = 4096
            };

            foreach (var command in group.DistinctBy(c => c.Name))
            {
                string description = string.IsNullOrWhiteSpace(command.Description) ? "`no help provided...`" : $"`{command.Description}`";
                commandBuilder.AppendLine($"`{context.Prefix}{command.Name}` -- {description}");
            }

            embedBuilder.WithFooter($"type {context.Prefix}{command?.QualifiedName} `command` to get a commands information");
            embedBuilder.WithDescription(commandBuilder.ToString().Trim());

            Page page = HouseHelpFormatterUtils.BuildPage(embedBuilder);

            pages.Add(page);
        }

        return this;
    }

    public override CommandHelpMessage Build()
    {
        return new();
    }
}

public static class HouseHelpFormatterUtils
{
    private static readonly Dictionary<Type, string> friendlyTypeNames = new()
    {
        [typeof(string)] = "text",
        [typeof(char)] = "character",
        [typeof(bool)] = "true/false",
        [typeof(byte)] = "small number",
        [typeof(sbyte)] = "small number",
        [typeof(short)] = "small number",
        [typeof(ushort)] = "small positive number",
        [typeof(int)] = "number",
        [typeof(uint)] = "positive number",
        [typeof(long)] = "large number",
        [typeof(ulong)] = "large positive number",
        [typeof(float)] = "decimal number",
        [typeof(double)] = "decimal number",
        [typeof(decimal)] = "decimal number",

        [typeof(DateTime)] = "date and time",
        [typeof(TimeSpan)] = "duration",
        [typeof(Guid)] = "unique ID",

        [typeof(DiscordUser)] = "user",
        [typeof(DiscordMember)] = "server member",
        [typeof(DiscordRole)] = "role",
        [typeof(DiscordChannel)] = "channel",
        [typeof(DiscordMessage)] = "message",
        [typeof(DiscordEmoji)] = "emoji",
        [typeof(DiscordGuild)] = "server",
        [typeof(DiscordIntegration)] = "integration",
        [typeof(DiscordApplication)] = "application",
        [typeof(DiscordWebhook)] = "webhook",
        [typeof(DiscordInvite)] = "invite",
        [typeof(DiscordVoiceState)] = "voice state",
        [typeof(DiscordActivity)] = "activity",
    };

    public static StringBuilder GetCommandDescription(Command command)
    {
        StringBuilder commandBuilder = new()
        {
            Capacity = 512
        };

        commandBuilder.AppendLine(command.Description == null || command.Description.Length == 0 ? "`no description provided...`" : command.Description);

        return commandBuilder;
    }

    public static StringBuilder GetShortCommandDescription(Command command)
    {
        StringBuilder commandBuilder = new()
        {
            Capacity = 256
        };

        string description;

        if (command.Description == null || command.Description.Length == 0)
        {
            description = "`no description provided...`";
        }
        else
        {
            description = command.Description;
        }

        commandBuilder.AppendLine(description);

        if (command.Aliases.Count > 5)
        {
            IOrderedEnumerable<string> aliases = command.Aliases.Take(5)
                .OrderByDescending(x => $"`{x}`");

            commandBuilder.Append(string.Join(", ", aliases) + (command.Aliases.Count > 5 ? "..." : ""));
        }

        return commandBuilder;
    }

    public static StringBuilder GetCommandAliases(Command command)
    {
        StringBuilder commandBuilder = new()
        {
            Capacity = 256
        };

        if (command.Aliases.Count != 0)
        {
            IOrderedEnumerable<string> aliases = command.Aliases.OrderByDescending(x => $"`{x}`");

            commandBuilder.Append(string.Join(", ", aliases));
        }
        else
        {
            commandBuilder.Append("`no aliases found...`");
        }

        return commandBuilder;
    }

    public static StringBuilder GetShortCommandAliases(Command command)
    {
        StringBuilder commandBuilder = new()
        {
            Capacity = 128
        };

        IReadOnlyList<string> aliases = command.Aliases;

        if (command.Aliases.Count > 5)
        {
            IOrderedEnumerable<string> orderedAliases = command.Aliases
                .OrderByDescending(x => $"`{x}`");

            string formatted = string.Join(", ", orderedAliases);
            commandBuilder.Append(formatted);
        }
        else
        {
            IOrderedEnumerable<string> orderedMinimalAliases = command.Aliases
                .Take(5)
                .OrderByDescending(x => $"`{x}`");

            string formatted = string.Join(", ", orderedMinimalAliases);
            commandBuilder.Append(formatted);
        }

        return commandBuilder;
    }

    public static StringBuilder GetCommandOverloads(Command command, CommandContext context)
    {
        StringBuilder commandBuilder = new()
        {
            Capacity = 1024
        };

        if (command.Overloads.Count != 0)
        {
            IOrderedEnumerable<CommandOverload> overloads = command.Overloads.OrderByDescending(x => x.Priority);

            foreach (CommandOverload overload in overloads.ThenBy(x => x.Arguments.Count))
            {
                IReadOnlyList<CommandArgument> commandArguments = overload.Arguments;
                string formattedArguments;

                if (commandArguments.Count != 0)
                {
                    formattedArguments = string.Join(' ', commandArguments.Select(arg => $"`{arg.Name} [{GetFriendlyTypeName(arg)}]`"));

                    commandBuilder.AppendLine($"`{context.Prefix}{command.QualifiedName}` {formattedArguments}");
                }
                else
                {
                    commandBuilder.AppendLine($"`{context.Prefix}{command.QualifiedName}`");
                }
            }
        }

        return commandBuilder;
    }

    public static Page BuildPage(DiscordEmbedBuilder embedBuilder)
    {
        return new(embed: embedBuilder);
    }

    public static string GetFriendlyTypeName(Type type)
    {
        if (!friendlyTypeNames.TryGetValue(type, out string? value) || value is null)
        {
            value = GetArgumentTypeName(type);
        }

        return value;
    }

    public static string GetFriendlyTypeName(CommandArgument argument)
    {
        return GetFriendlyTypeName(argument.Type);
    }

    public static string GetArgumentTypeName(Type type)
    {
        return type.Name.ToLowerInvariant();
    }

    public static string GetArgumentTypeName(CommandArgument argument)
    {
        return argument.Type.Name.ToLowerInvariant();
    }
}