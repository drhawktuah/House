using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using House.House.Attributes;
using House.House.Extensions;
using House.House.Services.Economy;
using House.House.Utils;

namespace House.House.Modules;

public class EconomyModule : BaseCommandModule
{
    public required HouseEconomyDatabase HouseEconomyDatabase { get; set; }

    [Command("register")]
    [Aliases("join", "joineconomy")]
    [Description("Go outside if you use this command")]
    [RequireGuild]
    public async Task RegisterPlayerAsync(CommandContext context)
    {
        await HouseEconomyDatabase.CreateUserAsync(context.User.Id);

        await context.RespondAsync($"welcome to house's economy, `{context.User.Username}`. don't be stupid");
    }

    [Command("balance")]
    [Aliases("bal")]
    [Description("Allows you to view your balance or someone else's")]
    [IsPlayer]
    [RequireGuild]
    public async Task Balance(CommandContext context, DiscordUser? user = null)
    {
        user ??= context.User;

        var found = await HouseEconomyDatabase.GetUserAsync(user.Id);
        var embed = new DiscordEmbedBuilder();

        embed.WithThumbnail(user.AvatarUrl);

        embed.WithTitle($"{user.Username}'s balance");
        embed.WithDescription($"Bank Balance: `{EconomyUtils.FormatCurrency(found.Bank)}`\nCash Balance: `{EconomyUtils.FormatCurrency(found.Cash)}`\n Total Balance: `{EconomyUtils.FormatCurrency(found.Bank + found.Cash)}`");

        embed.WithColor(EmbedUtils.EmbedColor);

        embed.WithFooter("balance signifies how much a person has", context.Client.CurrentUser.AvatarUrl);

        await context.RespondAsync(embed);
    }

    [Command("rob")]
    [Aliases("steal", "burglarize", "burglarise", "robplayer")]
    [Description("Allows you to become Eric Foreman!")]
    [IsPlayer]
    [RequireGuild]
    [Cooldown(1, 35, CooldownBucketType.User)]
    public async Task RobPlayerAsync(CommandContext context, DiscordMember victim)
    {
        var player = await HouseEconomyDatabase.GetUserAsync(victim.Id);

        if (player.Cash < 500)
        {
            await context.RespondAsync("you can't steal from someone with no money in their pockets...can't have shit in ppth");
            return;
        }

        if (victim == context.Member)
        {
            // this is actually a 1 in 1,000,001 chance, making it harder for you to duplicate your own wallet's currency
            if (Random.Shared.NextInt64(0, 1_000_000) == 1)
            {
                await HouseEconomyDatabase.ChangeBalanceAsync(victim.Id, player.Cash * 2);

                await context.RespondAsync($"i guess i am really dumb. you've duplicated your wallet amount");
                return;
            }
            else
            {
                await context.RespondAsync("nice try. you really thought i'd be flawed, cheers to you!");
                return;
            }
        }

        var toBeStealed = Random.Shared.NextLongWithinRange(500, player.Cash);

        var builder = new DiscordEmbedBuilder()
            .WithThumbnail(victim.AvatarUrl)
            .WithTitle(victim.Username)
            .WithColor(EmbedUtils.EmbedColor);

        if (toBeStealed <= 4_500)
        {
            builder.WithFooter("some goofy ahh taub robbery...i can hear the clown music playing", context.Client.CurrentUser.AvatarUrl);
        }
        else if (toBeStealed >= 10_000)
        {
            builder.WithFooter("a decent wilson scheme...nice work", context.Client.CurrentUser.AvatarUrl);
        }
        else if (toBeStealed == player.Cash)
        {
            builder.WithFooter("some house shit. you managed to steal pretty much everything...fly high lisa cuddy my beloved pigeon", context.Client.CurrentUser.AvatarUrl);
        }
        else
        {
            builder.WithFooter("i really don't know what to call this one, i guess nice work?", context.Client.CurrentUser.AvatarUrl);
        }

        await HouseEconomyDatabase.ModifyBalanceAsync(victim.Id, cash: -toBeStealed);
        await HouseEconomyDatabase.ModifyBalanceAsync(context.User.Id, cash: +toBeStealed);

        var changed = await HouseEconomyDatabase.GetUserAsync(victim.Id);

        builder.WithDescription(
            $"{context.User.Username} has stolen `{toBeStealed:##,#}` from {victim.Mention}\n-------------\n" +
            $"{victim.Mention} now has `{changed.Cash:##,#}`"
        );

        await context.RespondAsync(builder);
    }

    [Command("leaderboard")]
    [Aliases("getleaderboard")]
    [Description("Shows a leaderboard containing players and their stats")]
    [Cooldown(1, 30, CooldownBucketType.Global)]
    [IsPlayer]
    [RequireGuild]
    public async Task GetLeaderboardAsync(CommandContext context)
    {
        var players = await HouseEconomyDatabase.GetAllUsersAsync();

        if (players.Count == 0)
        {
            await context.RespondAsync("no have been players found...");
            return;
        }

        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("leaderboard")
            .WithColor(EmbedUtils.EmbedColor);

        StringBuilder builder = new();

        var topPlayers = players
            .OrderByDescending(x => x.Bank + x.Cash)
            .Take(20)
            .ToList();

        for (int i = 0; i < topPlayers.Count; i++)
        {
            var player = topPlayers[i];

            var user = await context.Client.GetUserAsync(player.ID, updateCache: true);

            if (user == null)
            {
                continue;
            }

            builder.AppendLine($"`[{(i + 1).Ordinal()}] - {user.Username}`");
            builder.AppendLine($"`Total balance: {EconomyUtils.FormatCurrency(player.Bank + player.Cash)}`\n");
        }

        embedBuilder.WithDescription(builder.ToString());

        await context.RespondAsync(embedBuilder);
    }

    [Command("deposit")]
    [Aliases("dep")]
    [Description("Allows you to hand in cash to your bank")]
    [IsPlayer]
    [RequireGuild]
    public async Task DepositAsync(CommandContext context, long amount = 500)
    {
        if (amount <= 0)
        {
            await context.RespondAsync("you can't deposit nothing");
            return;
        }

        var sender = await HouseEconomyDatabase.GetUserAsync(context.User.Id);

        if (amount > sender.Cash)
        {
            await context.RespondAsync($"you can't deposit more than `{EconomyUtils.FormatCurrency(sender.Cash)}` to your bank");
            return;
        }

        if (sender.Bank == long.MaxValue)
        {
            await context.RespondAsync("you can't deposit more money due to your account being full");
            return;
        }

        long availableSpace = long.MaxValue - sender.Cash;
        long actualTransfer = Math.Min(amount, availableSpace);

        sender.Cash -= actualTransfer;
        sender.Bank += actualTransfer;

        await HouseEconomyDatabase.UpdateUserAsync(sender);

        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("deposit")
            .WithDescription($"added `{EconomyUtils.FormatCurrency(actualTransfer)}` tokens to your bank account")
            .WithColor(EmbedUtils.EmbedColor);

        await context.RespondAsync(embedBuilder);
    }

    public async Task DepositAsync(CommandContext context, [RemainingText] string keyword)
    {
        if (!keyword.Equals("all", StringComparison.OrdinalIgnoreCase) &&
            !keyword.Equals("max", StringComparison.OrdinalIgnoreCase))
        {
            await context.RespondAsync("provide a valid amount (e.g: `all`, `max`) or use `deposit [amount]`");
            return;
        }

        var sender = await HouseEconomyDatabase.GetUserAsync(context.User.Id);

        if (sender.Cash <= 0)
        {
            await context.RespondAsync($"you can't deposit more than `{EconomyUtils.FormatCurrency(sender.Bank)}`");
            return;
        }

        if (sender.Bank == long.MaxValue)
        {
            await context.RespondAsync("your bank account is full--you cant deposit any more");
            return;
        }

        long availableSpace = long.MaxValue - sender.Bank;
        long actualTransfer = Math.Min(sender.Cash, availableSpace);

        sender.Cash -= actualTransfer;
        sender.Bank += actualTransfer;

        await HouseEconomyDatabase.UpdateUserAsync(sender);

        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("deposit")
            .WithDescription($"you deposited `{EconomyUtils.FormatCurrency(actualTransfer)}` tokens to your bank account")
            .WithColor(EmbedUtils.EmbedColor);

        await context.RespondAsync(embedBuilder);
    }

    [Command("withdraw")]
    [Aliases("with")]
    [Description("Allows you to withdraw money from your bank")]
    [IsPlayer]
    [RequireGuild]
    public async Task WithdrawAsync(CommandContext context, long amount = 500)
    {
        if (amount <= 0)
        {
            await context.RespondAsync("you can't withdraw nothing");
            return;
        }

        var sender = await HouseEconomyDatabase.GetUserAsync(context.User.Id);

        if (amount > sender.Bank)
        {
            await context.RespondAsync($"you can't withdraw more than `{EconomyUtils.FormatCurrency(sender.Bank)}`");
            return;
        }

        if (sender.Cash == long.MaxValue)
        {
            await context.RespondAsync("your wallet is full--you cant withdraw any more");
            return;
        }

        long availableSpace = long.MaxValue - sender.Cash;
        long actualTransfer = Math.Min(amount, availableSpace);

        sender.Bank -= actualTransfer;
        sender.Cash += actualTransfer;

        await HouseEconomyDatabase.UpdateUserAsync(sender);

        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("withdraw")
            .WithDescription($"you withdrew `{EconomyUtils.FormatCurrency(actualTransfer)}` from your bank")
            .WithColor(EmbedUtils.EmbedColor);

        await context.RespondAsync(embedBuilder);
    }

    public async Task WithdrawAsync(CommandContext context, [RemainingText] string keyword)
    {
        if (!keyword.Equals("all", StringComparison.OrdinalIgnoreCase) &&
            !keyword.Equals("max", StringComparison.OrdinalIgnoreCase))
        {
            await context.RespondAsync("provide a valid amount (e.g: `all`, `max`) or use `withdraw [amount]`");
            return;
        }

        var sender = await HouseEconomyDatabase.GetUserAsync(context.User.Id);

        if (sender.Bank <= 0)
        {
            await context.RespondAsync($"you can't withdraw more than `{EconomyUtils.FormatCurrency(sender.Bank)}`");
            return;
        }

        if (sender.Cash == long.MaxValue)
        {
            await context.RespondAsync("your wallet is full--you can't withdraw any more");
            return;
        }

        long availableSpace = long.MaxValue - sender.Cash;
        long actualTransfer = Math.Min(sender.Bank, availableSpace);

        sender.Bank -= actualTransfer;
        sender.Cash += actualTransfer;

        await HouseEconomyDatabase.UpdateUserAsync(sender);

        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("withdraw")
            .WithDescription($"you withdrew `{EconomyUtils.FormatCurrency(actualTransfer)}` from your bank")
            .WithColor(EmbedUtils.EmbedColor);

        await context.RespondAsync(embedBuilder);
    }

    [Command("transfer")]
    [Aliases("transfermoney", "transfertokens")]
    [Description("Transfers money from your bank to your wallet or vice versa or to another player.")]
    [IsPlayer]
    [RequireGuild]
    public async Task TransferMoneyAsync(CommandContext context, string account = "bank", string amount = "all", DiscordMember? member = null)
    {
        var sender = context.Member!;
        var isSelfTransfer = member == null || member.Id == sender.Id;
        var receiver = member ?? sender;

        var senderPlayer = await HouseEconomyDatabase.GetUserAsync(sender.Id);

        if (!isSelfTransfer)
        {
            var receiverPlayer = await HouseEconomyDatabase.GetUserAsync(receiver.Id);

            if (receiverPlayer == null)
            {
                await context.RespondAsync($"`{receiver.Username}` is not apart of the economy");
                return;
            }
        }

        bool fromBank;

        if (account.Equals("bank", StringComparison.OrdinalIgnoreCase))
        {
            fromBank = true;
        }
        else if (account.Equals("wallet", StringComparison.OrdinalIgnoreCase) || account.Equals("cash", StringComparison.OrdinalIgnoreCase))
        {
            fromBank = false;
        }
        else
        {
            await context.RespondAsync("Invalid account type. Use `bank` or `wallet`.");
            return;
        }

        long senderBalance = fromBank ? senderPlayer.Bank : senderPlayer.Cash;
        long transferAmount;

        string lowerAmount = amount.ToLowerInvariant();

        if (lowerAmount is "all" or "max")
        {
            transferAmount = senderBalance;
        }
        else if (lowerAmount == "half")
        {
            transferAmount = senderBalance / 2;
        }
        else if (lowerAmount == "quarter")
        {
            transferAmount = senderBalance / 4;
        }
        else if (lowerAmount.EndsWith('%') && int.TryParse(lowerAmount.TrimEnd('%'), out int percent) && percent >= 1 && percent <= 100)
        {
            transferAmount = senderBalance * percent / 100;
        }
        else if (long.TryParse(lowerAmount, out long parsedAmount) && parsedAmount > 0)
        {
            transferAmount = parsedAmount;
        }
        else
        {
            await context.RespondAsync("invalid amount. use a number, percentage, or a keyword like `max`, `all`, `half`, or `quarter`");
            return;
        }

        if (transferAmount <= 0)
        {
            await context.RespondAsync("you must transfer more than 0");
            return;
        }

        if (transferAmount > senderBalance)
        {
            await context.RespondAsync("you don't have enough funds to transfer that amount");
            return;
        }

        if (isSelfTransfer)
        {
            if (fromBank)
            {
                await HouseEconomyDatabase.ModifyBalanceAsync(sender.Id, bank: -transferAmount, cash: transferAmount);
            }
            else
            {
                await HouseEconomyDatabase.ModifyBalanceAsync(sender.Id, bank: transferAmount, cash: -transferAmount);
            }

            await context.RespondAsync($"you transferred `{transferAmount:N0}` from your {(fromBank ? "bank" : "wallet")} to your {(fromBank ? "wallet" : "bank")}");
        }
        else
        {
            await HouseEconomyDatabase.ModifyBalanceAsync(sender.Id, bank: fromBank ? -transferAmount : 0, cash: fromBank ? 0 : -transferAmount);
            await HouseEconomyDatabase.ModifyBalanceAsync(receiver.Id, transferAmount);

            await context.RespondAsync($"you transferred `{transferAmount:N0}` {(fromBank ? "from your bank" : "from your wallet")} to `{receiver.Username}`");
        }
    }

    public async Task TransferMoneyAsync(CommandContext context, DiscordMember member, long amount = 500)
    {
        if (member.Id == context.User.Id)
        {
            await context.RespondAsync("you can't send money to yourself");
            return;
        }

        if (amount <= 0)
        {
            await context.RespondAsync($"you can't send nothing to {member.Mention}");
            return;
        }

        var sender = await HouseEconomyDatabase.GetUserAsync(context.User.Id);
        var receiver = await HouseEconomyDatabase.GetUserAsync(member.Id);

        if (amount > sender.Cash)
        {
            await context.RespondAsync($"you can't send more than `{EconomyUtils.FormatCurrency(amount)}` to {member.Mention}");
            return;
        }

        if (receiver.Cash == long.MaxValue)
        {
            await context.RespondAsync($"{member.Mention} has the max amount of money in their account {EconomyUtils.FormatCurrency(amount)}");
            return;
        }

        long availableSpace = long.MaxValue - receiver.Cash;
        long actualTransfer = Math.Min(amount, availableSpace);

        sender.Cash -= actualTransfer;
        receiver.Cash += actualTransfer;

        await HouseEconomyDatabase.UpdateUserAsync(sender);
        await HouseEconomyDatabase.UpdateUserAsync(receiver);

        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("cash transaction")
            .WithDescription($"{context.User.Mention} gave {member.Mention} `{EconomyUtils.FormatCurrency(actualTransfer)}`")
            .WithColor(EmbedUtils.EmbedColor);

        await context.RespondAsync(embedBuilder);
    }
}
