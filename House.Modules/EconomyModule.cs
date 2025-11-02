using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using House.House.Attributes;
using House.House.Extensions;
using House.House.Services.Economy;
using House.House.Utils;

namespace House.House.Modules;

[Description("House's economy games")]
public sealed class EconomyGamesModule : BaseCommandModule
{
    public required HouseEconomyDatabase HouseEconomyDatabase { get; set; }

    [Command("vicodincatch")]
    [Aliases("catchvicodin", "forgewilsonssignature")]
    [Description("Catch the vicodin! See how far you can get!")]
    [IsPlayer]
    public async Task CatchVicodinAsync(CommandContext context)
    {
        const string JoinEmojiUnicode = "🏠";

        const int MaxLobbySize = 20;
        const int LobbyTimeoutSeconds = 15;
        const int ReactionPollInterval = 2_000;

        const int ButtonRows = 3;
        const int ButtonColumns = 4;
        const int RoundTimeoutSeconds = 3;

        const string DefaultLabel = "Vicodin not here";
        const string ActiveLabel = "RIGHT HERE!";
        const string FoundLabel = "You found Vicodin!";

        var interactivity = context.Client.GetInteractivity();

        DiscordEmoji joinEmoji = DiscordEmoji.FromUnicode(JoinEmojiUnicode);
        TimeSpan timeout = TimeSpan.FromSeconds(LobbyTimeoutSeconds);

        var message = await context.RespondAsync($"You have `{LobbyTimeoutSeconds}` seconds to join before starting! Any player can participate. React with '{JoinEmojiUnicode}' to participate.");
        await message.CreateReactionAsync(joinEmoji);

        HashSet<DiscordUser> users = [context.User];
        HashSet<ulong> seen = [context.User.Id];

        DateTime startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var reactionUsers = await message.GetReactionsAsync(joinEmoji);

            foreach (var user in reactionUsers)
            {
                if (user.IsBot || seen.Contains(user.Id))
                {
                    continue;
                }

                seen.Add(user.Id);

                var player = await HouseEconomyDatabase.TryGetPlayerAsync(user.Id);
                if (player == null)
                {
                    await context.Channel.SendMessageAsync($"{user.Username} is not part of the economy. Ignoring reaction");
                    continue;
                }

                if (users.Count >= MaxLobbySize)
                {
                    await context.Channel.SendMessageAsync($"Sorry, {user.Username}! Lobby is now full.");
                    break;
                }

                users.Add(user);

                await context.Channel.SendMessageAsync($"{user.Username} joined!");
            }

            if (users.Count >= MaxLobbySize)
            {
                break;
            }

            await Task.Delay(ReactionPollInterval);
        }

        if (users.Count == 0)
        {
            await context.Channel.SendMessageAsync("No one joined the lobby");
            return;
        }

        string joinedPlayers = string.Join(", ", users.Select(u => u.Mention));
        await context.Channel.SendMessageAsync($"Players joined: {joinedPlayers}");

        await Task.Delay(1000);

        int countdownSeconds = 5;

        var initialBuilder = new DiscordMessageBuilder()
            .WithContent($"https://cdn.discordapp.com/attachments/1399807023637467216/1409406116315533383/vXiXmghr6.jpg?ex=68ad433b&is=68abf1bb&hm=53720874ee9bda93bbad5f8735248666a2a216e8abcb969426c93e3e297206bd& \nCatch my vicodin, you dare? Ye olde black Foreman won't be *colored* impressed. Ha, get it? He's black.");

        await context.Channel.SendMessageAsync(initialBuilder);

        var countdownMessage = await context.Channel.SendMessageAsync($"Anyways, game begins in {countdownSeconds} seconds");

        for (int i = countdownSeconds - 1; i >= 1; i--)
        {
            await Task.Delay(1000);

            var newMessage = new DiscordMessageBuilder()
                .WithContent($"Anyways, game begins in {i} second{(i != 1 ? "s" : "")}");

            await countdownMessage.ModifyAsync(newMessage);
        }

        int maxVicodinFound = RandomNumberGenerator.GetInt32(100, 1_000);

        Console.WriteLine(maxVicodinFound);

        Dictionary<ulong, int> totalVicodinFound = users.ToDictionary(k => k.Id, v => 0);

        await context.Channel.SendMessageAsync("Preparing the vicodin grid...");

        int currentRound = 1;
        int? foundIndex = null;

        int maxTimeoutTries = 3;
        int currentTimeoutTries = 0;

        bool gameCancelled = false;

        DiscordMessage? gameMessage = null;
        DiscordMessage? roundResultMessage = null;

        while (currentRound <= maxVicodinFound)
        {
            int activeIndex = RandomNumberGenerator.GetInt32(0, ButtonRows * ButtonColumns);

            List<DiscordActionRowComponent> buttons = EconomyUtils.BuildGameGrid(
                ButtonRows, ButtonColumns,
                DefaultLabel, ActiveLabel, FoundLabel,
                activeIndex,
                foundIndex,
                disableAll: false
            );

            if (gameMessage == null)
            {
                gameMessage = await context.Channel.SendMessageAsync(
                    new DiscordMessageBuilder()
                        .WithContent($"Round {currentRound}: Find Vicodin!")
                        .AddComponents(buttons)
                );
            }
            else
            {
                var gameMessageBuilder = new DiscordMessageBuilder()
                    .WithContent($"Round {currentRound}: Find Vicodin!")
                    .AddComponents(buttons);

                await gameMessage.ModifyAsync(gameMessageBuilder);
            }

            var interactionResult = await interactivity.WaitForButtonAsync(gameMessage, TimeSpan.FromSeconds(RoundTimeoutSeconds));

            if (interactionResult.TimedOut)
            {
                currentTimeoutTries++;

                var disabledButtons = EconomyUtils.BuildGameGrid(
                    ButtonRows,
                    ButtonColumns,
                    DefaultLabel,
                    ActiveLabel,
                    FoundLabel,
                    disableAll: true
                );

                var timeoutBuilder = new DiscordMessageBuilder()
                    .WithContent($"Round {currentRound}: Time's up! Nobody found Vicodin")
                    .AddComponents(disabledButtons);

                await gameMessage.ModifyAsync(timeoutBuilder);

                foundIndex = null;

                await Task.Delay(1500);

                if (currentTimeoutTries >= maxTimeoutTries)
                {
                    await context.Channel.SendMessageAsync("No one has found Vicodin for several rounds. Game cancelled due to inactivity");
                    gameCancelled = true;
                    break;
                }
            }
            else
            {
                currentTimeoutTries = 0;

                var interaction = interactionResult.Result.Interaction;
                var user = interaction.User;

                if (!users.Any(u => u.Id == user.Id))
                {
                    continue;
                }

                int pointsEarned = RandomNumberGenerator.GetInt32(100, 200);
                if (!totalVicodinFound.ContainsKey(user.Id))
                    totalVicodinFound[user.Id] = 0;

                totalVicodinFound[user.Id] += pointsEarned;

                Console.WriteLine($"{user.Username} total Vicodin: {totalVicodinFound[user.Id]}");

                foundIndex = activeIndex;

                await interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                string resultText = $"{user.Mention} found Vicodin and earned {pointsEarned} points! 🎉";

                if (roundResultMessage == null)
                {
                    roundResultMessage = await context.Channel.SendMessageAsync(resultText);
                }
                else
                {
                    await roundResultMessage.ModifyAsync(resultText);
                }

                await Task.Delay(2500);
            }

            currentRound++;

            if (totalVicodinFound.Values.Any(v => v >= maxVicodinFound))
            {
                break;
            }
        }

        if (gameMessage != null)
        {
            var disabledGrid = EconomyUtils.BuildGameGrid(
                ButtonRows,
                ButtonColumns,
                DefaultLabel,
                ActiveLabel,
                FoundLabel,
                disableAll: true
            );

            var finalBuilder = new DiscordMessageBuilder()
                .WithContent("Game ended!")
                .AddComponents(disabledGrid);

            await gameMessage.ModifyAsync(finalBuilder);
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Color = EmbedUtils.EmbedColor
        };

        if (gameCancelled || totalVicodinFound.All(kvp => kvp.Value == 0))
        {
            embedBuilder.Title = "Nobody found my vicodin...?";
            embedBuilder.Description = $"{joinedPlayers} you ALL suck bajee balls";
            embedBuilder.WithImageUrl("https://cdn.discordapp.com/attachments/1399807023637467216/1409737283715858494/I9P04zMZM.png?ex=68ae77a8&is=68ad2628&hm=d5e65a332e8bc928fe57b09b948ec8e1b4beb530269684b7fa90c71176d883ae&");
        }
        else
        {
            var balanceUpdates = new List<(ulong ID, long Tokens)>();

            var leaderboardEntries = totalVicodinFound
                .Where(kvp => kvp.Value > 0)
                .OrderByDescending(kvp => kvp.Value)
                .Select((kvp, index) =>
                {
                    var user = users.FirstOrDefault(u => u.Id == kvp.Key);
                    string username = user?.Username ?? $"{kvp.Key}";

                    return $"[{index + 1}]. {username} - {EconomyUtils.FormatCurrency(kvp.Value)} Vicodin";
                });

            embedBuilder.Title = "Excellent. My vicodin. Here's your leaderboard";
            embedBuilder.Description = string.Join("\n", leaderboardEntries);
            embedBuilder.WithImageUrl("https://cdn.discordapp.com/attachments/1399807023637467216/1409737240879435937/3VIABBOkw.png?ex=68ae779d&is=68ad261d&hm=b6de53c1b88d11083ffded9e6be715722325dff299fe0d9855ac101970cc560d&");

            foreach (var (kvp, index) in totalVicodinFound
                .Where(kvp => kvp.Value > 0)
                .OrderByDescending(kvp => kvp.Value)
                .Select((kvp, index) => (kvp, index)))
            {
                var user = users.FirstOrDefault(u => u.Id == kvp.Key);
                string username = user?.Username ?? $"{kvp.Key}";

                int vicodinFound = kvp.Value;
                long tokensEarned = EconomyUtils.CalculateTokensFromVicodin(vicodinFound);

                embedBuilder.AddField(
                    name: username,
                    value: $"{EconomyUtils.FormatCurrency(tokensEarned)} tokens",
                    inline: true
                );

                balanceUpdates.Add((kvp.Key, tokensEarned));
            }

            foreach (var (userId, tokens) in balanceUpdates)
            {
                try
                {
                    await HouseEconomyDatabase.ModifyBalanceAsync(userId, cash: tokens);
                }
                catch (UserNotFoundException)
                {
                    await context.RespondAsync($"<@{userId}> contact my owner about your tokens. I was not able to add {EconomyUtils.FormatCurrency(tokens)} to your balance");
                }
            }
        }

        await context.Channel.SendMessageAsync(embedBuilder);
    }
}

public class EconomyModule : BaseCommandModule
{
    public required HouseEconomyDatabase HouseEconomyDatabase { get; set; }

    [Command("create")]
    [Aliases("join", "joineconomy")]
    [Description("Go outside if you use this command")]
    [RequireGuild]
    public async Task CreateProfileAsync(CommandContext context)
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
    [Aliases("steal", "burglarize", "blackerize", "blackerise", "burglarise", "robplayer")]
    [Description("Basically pulls a George Floyd on players with cash in their wallets")]
    [IsPlayer]
    [RequireGuild]
    [Cooldown(1, 35, CooldownBucketType.User)]
    public async Task RobPlayerAsync(CommandContext context, DiscordMember victim)
    {
        var player = await HouseEconomyDatabase.GetUserAsync(victim.Id);

        if (player.Cash < 500)
        {
            await context.RespondAsync("you can't steal from someone with no money in their pockets...can't have shit in detroit");
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

        // gta 5 canonical lore, lamar and franklin only pulled off a heist (technically, more of a robbery) of up to 2 dye pack stacks split.
        // i've only added an extra 2,500 because of lore
        var builder = new DiscordEmbedBuilder()
            .WithThumbnail(victim.AvatarUrl)
            .WithTitle(victim.Username)
            .WithColor(EmbedUtils.EmbedColor);

        if (toBeStealed <= 4_500)
        {
            builder.WithFooter("some franklin and lamar ass robbery", context.Client.CurrentUser.AvatarUrl);
        }
        else if (toBeStealed >= 10_000)
        {
            builder.WithFooter("some michael ass robbery, nice job", context.Client.CurrentUser.AvatarUrl);
        }
        else if (toBeStealed == player.Cash)
        {
            builder.WithFooter("some aiden pearce + damien brenks shit. you've stolen everything, excellent work", context.Client.CurrentUser.AvatarUrl);
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
            await context.RespondAsync("Invalid amount. Use a number, percentage, or a keyword like `max`, `all`, `half`, or `quarter`");
            return;
        }

        if (transferAmount <= 0)
        {
            await context.RespondAsync("You must transfer more than 0");
            return;
        }

        if (transferAmount > senderBalance)
        {
            await context.RespondAsync("You don't have enough funds to transfer that amount");
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

            await context.RespondAsync($"You transferred `{transferAmount:N0}` from your {(fromBank ? "bank" : "wallet")} to your {(fromBank ? "wallet" : "bank")}");
        }
        else
        {
            await HouseEconomyDatabase.ModifyBalanceAsync(sender.Id, bank: fromBank ? -transferAmount : 0, cash: fromBank ? 0 : -transferAmount);
            await HouseEconomyDatabase.ModifyBalanceAsync(receiver.Id, transferAmount);

            await context.RespondAsync($"You transferred `{transferAmount:N0}` {(fromBank ? "from your bank" : "from your wallet")} to `{receiver.Username}`");
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

    [Command("heist")]
    [Aliases("heistperson", "heistonperson")]
    [Description("Perform a heist on a person (GTA 5 ahh robbery)")]
    [Cooldown(1, 600, CooldownBucketType.User)]
    [IsPlayer]
    [RequireGuild]
    public async Task HeistAsync(CommandContext context, DiscordMember member)
    {
        var victim = await HouseEconomyDatabase.TryGetPlayerAsync(member.Id);

        if (victim == null)
        {
            await context.RespondAsync($"{member.Username} is not in my economy");
            return;
        }

        if (victim.Bank <= 1_000)
        {
            await context.RespondAsync($"You cant steal off of some broke ass loser, lmao");
            return;
        }

        const string JoinEmojiUnicode = "🏠";

        const int MaxPlayers = 30;
        const int MinPlayers = 4;
        const int LobbyTimeoutSeconds = 15;

        const decimal MaxStealFraction = 0.30m;

        var interactivity = context.Client.GetInteractivity();
        var joinEmoji = DiscordEmoji.FromUnicode(JoinEmojiUnicode);
        var timeout = TimeSpan.FromSeconds(LobbyTimeoutSeconds);

        var users = new HashSet<DiscordUser>
        {
            context.User
        };

        var seen = new HashSet<ulong>
        {
            context.User.Id,
        };

        var embed = new DiscordEmbedBuilder()
            .WithTitle("House's Heist Lobby")
            .WithDescription($"React with {joinEmoji} to join!" +
                                $"You have **{LobbyTimeoutSeconds} seconds**.\n" +
                                $"Minimum players: {MinPlayers} | Maximum: {MaxPlayers}")
            .AddField("Joined players", $"{context.User.Mention}")
            .WithColor(DiscordColor.Gold)
            .WithTimestamp(DateTime.UtcNow);

        var message = await context.RespondAsync(embed);
        await message.CreateReactionAsync(joinEmoji);

        var reactions = await interactivity.CollectReactionsAsync(
            message,
            timeout
        );

        foreach (var reaction in reactions)
        {
            if (reaction.Emoji != joinEmoji)
            {
                continue;
            }

            foreach (var user in reaction.Users)
            {
                if (user.IsBot || seen.Contains(user.Id))
                {
                    continue;
                }

                seen.Add(user.Id);

                var player = await HouseEconomyDatabase.TryGetPlayerAsync(user.Id);
                if (player == null)
                {
                    continue;
                }

                if (users.Count >= MaxPlayers)
                {
                    await context.Channel.SendMessageAsync($"Sorry {user.Username}, the lobby is full");
                    break;
                }

                users.Add(user);
            }

            if (users.Count >= MaxPlayers)
            {
                break;
            }
        }

        if (users.Any(u => u.Id == member.Id))
        {
            users.RemoveWhere(u => u.Id == member.Id);
        }

        if (users.Count < MinPlayers)
        {
            await context.Channel.SendMessageAsync("Not enough players joined. Heist is now cancelled");
            return;
        }

        string joinedPlayers = string.Join(", ", users.Select(u => u.Mention));

        var finalEmbed = new DiscordEmbedBuilder(embed)
            .WithTitle("Heist Starting")
            .WithDescription("Lobby is now closed")
            .AddField("Participants", joinedPlayers)
            .WithColor(DiscordColor.SpringGreen)
            .WithTimestamp(DateTime.UtcNow);

        decimal victimBalance = victim.Bank;

        int participantCount = users.Count;

        long totalStolenCents = (long)Math.Floor((double)(victimBalance * MaxStealFraction * 100m));

        if (totalStolenCents <= 0)
        {
            await context.Channel.SendMessageAsync($"{member.Username}'s balance is too small");
            return;
        }

        long shareCents = totalStolenCents / participantCount;

        if (shareCents <= 0)
        {
            await context.Channel.SendMessageAsync("Share per participant is 0. Cannot proceed with heist");
            return;
        }

        await message.ModifyAsync(embed: finalEmbed.Build());
        await Task.Delay(2_000);
        await context.Channel.SendMessageAsync("The heist begins!");

        long distributedTotalCents = shareCents * participantCount;

        decimal totalStolen = distributedTotalCents / 100m;
        decimal perPlayerShare = shareCents / 100m;
        decimal victimRemaining = (long)Math.Max(0m, victimBalance - totalStolen);

        long victimRemainingCents = (long)Math.Floor(victimRemaining * 100m);

        await HouseEconomyDatabase.ModifyBalanceAsync(member.Id, victimRemainingCents);

        foreach (var user in users)
        {
            var player = await HouseEconomyDatabase.TryGetPlayerAsync(user.Id);

            if (player == null)
            {
                continue;
            }

            long currentBalanceCents = (long)Math.Floor(player.Bank * 100m);
            long newBalanceCents = currentBalanceCents + (long)Math.Floor(perPlayerShare * 100m);

            await HouseEconomyDatabase.ModifyBalanceAsync(user.Id, newBalanceCents);
        }
    }
}
