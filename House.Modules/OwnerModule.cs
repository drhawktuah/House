using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using House.House.Attributes;
using House.House.Services.Database;
using House.House.Services.Economy;
using House.House.Utils;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace House.House.Modules;

[Group("owner")]
[Description("Owner commands for database management")]
[Hidden]
public sealed class OwnerModule : BaseCommandModule
{
    private readonly WhitelistedUserRepository whitelistRepo;
    private readonly BlacklistedUserRepository blacklistRepo;
    private readonly StaffUserRepository staffRepo;

    public OwnerModule(
        WhitelistedUserRepository whitelistRepo,
        BlacklistedUserRepository blacklistRepo,
        StaffUserRepository staffRepo)
    {
        this.whitelistRepo = whitelistRepo;
        this.blacklistRepo = blacklistRepo;
        this.staffRepo = staffRepo;
    }

    [Command("addwhitelist")]
    [Description("Adds a user to the whitelist")]
    [IsOwner]
    [RequireGuild]
    public async Task AddWhitelistAsync(CommandContext ctx, string username, [RemainingText] string reason = "No reason provided")
    {
        var user = new WhitelistedUser
        {
            Username = username,
            Reason = reason,
            WhitelistedAt = DateTime.UtcNow
        };

        await whitelistRepo.AddAsync(user);
        await ctx.RespondAsync($"User '{username}' added to whitelist");
    }

    [Command("addwhitelist")]
    [Description("Adds a user to the whitelist by member")]
    [IsOwner]
    [RequireGuild]
    public async Task AddWhitelistAsync(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "No reason provided")
    {
        var user = new WhitelistedUser
        {
            ID = member.Id,
            Username = member.Username,
            Reason = reason,
            WhitelistedAt = DateTime.UtcNow
        };

        await whitelistRepo.AddAsync(user);
        await ctx.RespondAsync($"User '{user.Username}' added to whitelist");
    }

    [Command("removewhitelist")]
    [Description("Removes a user from the whitelist")]
    [IsOwner]
    [RequireGuild]
    public async Task RemoveWhitelistAsync(CommandContext ctx, DiscordMember member)
    {
        await whitelistRepo.DeleteAsync(member.Id);
        await ctx.RespondAsync($"User '{member.Username}' removed from whitelist");
    }

    [Command("addblacklist")]
    [Description("Adds a user to the blacklist")]
    [IsStaff(Position.Admin)]
    [RequireGuild]
    public async Task AddBlacklistAsync(CommandContext ctx, string username, [RemainingText] string reason = "No reason provided")
    {
        var user = new BlacklistedUser
        {
            Username = username,
            Reason = reason,
            BlacklistedAt = DateTime.UtcNow
        };

        await blacklistRepo.AddAsync(user);
        await ctx.RespondAsync($"User '{username}' added to blacklist");
    }

    [Command("addblacklist")]
    [Description("Adds a user to the blacklist by member")]
    [IsStaff(Position.Admin)]
    [RequireGuild]
    public async Task AddBlacklistAsync(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "No reason provided")
    {
        if (member.Id == ctx.User.Id)
        {
            await ctx.RespondAsync("You cannot blacklist yourself");
            return;
        }

        var user = new BlacklistedUser
        {
            ID = member.Id,
            Username = member.Username,
            Reason = reason,
            BlacklistedAt = DateTime.UtcNow
        };

        await blacklistRepo.AddAsync(user);
        await ctx.RespondAsync($"User '{user.Username}' added to blacklist");
    }

    [IsStaff(Position.Admin)]
    [Command("removeblacklist")]
    [Description("Removes a user from the blacklist")]
    [RequireGuild]
    public async Task RemoveBlacklistAsync(CommandContext ctx, DiscordMember member)
    {
        await blacklistRepo.DeleteAsync(member.Id);
        await ctx.RespondAsync($"User '{member.Username}' removed from blacklist");
    }

    [Command("addstaff")]
    [IsOwner]
    [Description("Adds a user as staff")]
    [RequireGuild]
    public async Task AddStaffAsync(CommandContext ctx, string username, Position position)
    {
        var user = new StaffUser
        {
            Username = username,
            Position = position
        };

        await staffRepo.AddAsync(user);
        await ctx.RespondAsync($"User '{username}' added as {position}");
    }

    [Command("addstaff")]
    [Description("Adds a user as staff by member")]
    [IsOwner]
    [RequireGuild]
    public async Task AddStaffAsync(CommandContext ctx, DiscordMember member, Position position)
    {
        if (member.Id == ctx.User.Id)
        {
            await ctx.RespondAsync("You are already staff");
            return;
        }

        var user = new StaffUser
        {
            ID = member.Id,
            Username = member.Username,
            Position = position
        };

        await staffRepo.AddAsync(user);
        await ctx.RespondAsync($"User '{user.Username}' added as {position}");
    }

    [Command("removestaff")]
    [IsOwner]
    [Description("Removes a user from staff by member")]
    [RequireGuild]
    public async Task RemoveStaffAsync(CommandContext ctx, DiscordMember member)
    {
        if (member.Id == ctx.User.Id)
        {
            await ctx.RespondAsync("You cannot remove yourself from staff");
            return;
        }

        await staffRepo.DeleteAsync(member.Id);
        await ctx.RespondAsync($"User '{member.Username}' removed from staff");
    }
}

[Group("economyowner")]
[Description("Authority over players/objects in House's economy")]
[Hidden]
public sealed class EconomyOwnerModule : BaseCommandModule
{
    private readonly HouseEconomyDatabase economyDatabase;

    public EconomyOwnerModule(HouseEconomyDatabase economyDatabase)
    {
        this.economyDatabase = economyDatabase;
    }

    [Command("resetall")]
    [IsOwner]
    [Description("resets the balance of ALL users, requires confirmation")]
    [RequireGuild]
    public async Task ResetAllBalanceAsync(CommandContext context)
    {
        List<HouseEconomyUser> economyUsers = await economyDatabase.GetAllUsersAsync();

        if (economyUsers.All(x => (x.Bank + x.Cash) == 0))
        {
            await context.RespondAsync("cannot reset the balance of all players because they literally have nothing");
            return;
        }

        await context.RespondAsync($"are you sure you want to reset the balance of `{economyUsers.Count}` players?");

        var interactivity = context.Client.GetInteractivity();
        var waited = await interactivity.WaitForMessageAsync(x => x.Channel == context.Channel && x.Author == context.User, TimeSpan.FromSeconds(20));

        if (waited.TimedOut)
        {
            await context.RespondAsync($"`timed out whilst authenticating to reset balance of {economyUsers.Count} users`");
            return;
        }

        var result = waited.Result.Content.Trim().ToLowerInvariant();

        if (result is not ("yes" or "y"))
        {
            await context.RespondAsync($"`request to reset the balance of {economyUsers.Count} has been cancelled`");
            return;
        }

        long total = 0;

        foreach (HouseEconomyUser economyUser in economyUsers)
        {
            if (economyUser.Bank > 0)
            {
                total += economyUser.Bank;
                economyUser.Bank = 0;
            }

            if (economyUser.Cash > 0)
            {
                total += economyUser.Cash;
                economyUser.Cash = 0;
            }

            await economyDatabase.UpdateUserAsync(economyUser);
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Cleared")
            .WithDescription($"removed `{EconomyUtils.FormatCurrency(total)}` from all users")
            .WithThumbnail(context.Client.CurrentUser.AvatarUrl);

        await context.Channel.SendMessageAsync(embed);
    }
}

[Description("House's module")]
[Hidden]
public sealed class HouseModule : BaseCommandModule
{
    private Func<object?>? lastResult = null;

    [Command("eval")]
    [Hidden]
    [IsOwner]
    public async Task EvalAsync(CommandContext context, [RemainingText] string body)
    {
        async Task SendExceptionAsync(string content)
        {
            const int MaxDiscordLength = 2000;
            const string CodeBlockWrapper = "```cs\n";
            const string CodeBlockFooter = "\n```";

            int maxContentLength = MaxDiscordLength - CodeBlockWrapper.Length - CodeBlockFooter.Length;

            if (content.Length > maxContentLength)
            {
                content = string.Concat(content.AsSpan(0, maxContentLength - 3), "...");
            }

            var message = await context.RespondAsync($"{CodeBlockWrapper}{content}{CodeBlockFooter}");
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            await context.RespondAsync("`body cannot be null`");
            return;
        }

        string code = CleanCode(body);

        code = $@"
async Task<object?> Eval() {{
{code}
return null;
}}

return await Eval();
";

        var env = new Dictionary<string, object?>()
        {
            ["bot"] = context.Client,
            ["context"] = context,
            ["channel"] = context.Channel,
            ["author"] = context.User,
            ["guild"] = context.Guild,
            ["message"] = context.Message,
            ["_"] = lastResult
        };

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Distinct();

        var options = ScriptOptions.Default
            .AddReferences(assemblies)
            .AddImports(
                "System",
                "System.Linq",
                "System.Collections.Generic",
                "System.Threading.Tasks",
                "DSharpPlus",
                "DSharpPlus.Entities",
                "DSharpPlus.CommandsNext"
            );

        try
        {
            var globals = new EvalGlobals(env);

            if (!code.Contains("return") && !code.Contains("await") && !code.TrimEnd().EndsWith(';'))
            {
                code = $"return {code};";
            }

            if (!code.Contains("await") && !code.Contains("return") && !code.Contains(';'))
            {
                code = $"return {code};";
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var script = CSharpScript.Create(code, options, typeof(EvalGlobals));
            var state = await script.RunAsync(globals, cts.Token);

            var result = state.ReturnValue;
            lastResult = () => result;

            string output = globals.ConsoleOutput ?? "";

            if (result != null)
            {
                if (output.Length > 0)
                {
                    output += "\n";
                }

                output += result;
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                output = "null";
            }

            const int MaxDiscordLength = 2000;
            const string CodeBlockWrapper = "```cs\n";
            const string CodeBlockFooter = "\n```";

            int maxContentLength = MaxDiscordLength - CodeBlockWrapper.Length - CodeBlockFooter.Length;

            output ??= "null";

            if (output.Length > maxContentLength)
            {
                output = string.Concat(output.AsSpan(0, maxContentLength - 3), "...");
            }

            var message = await context.RespondAsync($"{CodeBlockWrapper}{output}{CodeBlockFooter}");
            await message.CreateReactionAsync(DiscordEmoji.FromUnicode("✅"));
        }

        catch (CompilationErrorException ex)
        {
            await SendExceptionAsync(string.Join("\n", ex.Diagnostics));
        }
        catch (Exception ex)
        {
            await SendExceptionAsync(string.Join("\n", ex));
        }
    }

    public class EvalGlobals
    {
        private readonly Dictionary<string, object?> env;
        private readonly StringWriter output;

        public EvalGlobals(Dictionary<string, object?> env)
        {
            this.env = env;

            output = new();
            Console.SetOut(output);
        }

        public object? this[string key] => env.TryGetValue(key, out var value) ? value : null;
        public string ConsoleOutput => output.ToString();

        public DiscordClient Bot => (DiscordClient)env["bot"]!;
        public CommandContext Context => (CommandContext)env["context"]!;
        public DiscordChannel Channel => (DiscordChannel)env["channel"]!;
        public DiscordUser Author => (DiscordUser)env["author"]!;
        public DiscordGuild Guild => (DiscordGuild)env["guild"]!;
        public DiscordMessage Message => (DiscordMessage)env["message"]!;
        public Func<object?>? _ => env["_"] as Func<object?>;

#pragma warning disable IDE1006 // Naming Styles

        public DiscordClient bot => Bot;
        public CommandContext context => Context;
        public DiscordChannel channel => Channel;
        public DiscordUser author => Author;
        public DiscordGuild guild => Guild;
        public DiscordMessage message => Message;

#pragma warning restore IDE1006 // Naming Styles
    }

    private static string CleanCode(string content)
    {
        if (content.StartsWith("```") && content.EndsWith("```"))
        {
            string[] lines = content.Split("\n");

            if (lines.Length >= 2)
            {
                IEnumerable<string> codeLines = lines.Skip(1).Take(lines.Length - 2);

                return string.Join("\n", codeLines).Trim();
            }

            return string.Empty;
        }

        if (content.StartsWith('`') && content.EndsWith('`'))
        {
            return content[1..^1].Trim();
        }

        return content.Trim();
    }
}