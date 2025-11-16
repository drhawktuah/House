using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using House.House.Extensions;
using House.House.Services.Economy;
using House.House.Services.Economy.General;
using House.House.Services.Economy.Items;
using House.House.Services.Economy.Vendors;
using House.House.Utils;

namespace House.House.Modules;

/*
        var vendor = VendorPresets.VendorPool.FirstOrDefault(v => v.Name.Equals(vendorName, StringComparison.OrdinalIgnoreCase));
        if (vendor == null)
        {
            await ctx.RespondAsync($"Vendor `{vendorName}` not found.");
            return;
        }

        if (vendor.Inventory.Count == 0)
        {
            await ctx.RespondAsync($"ℹ`{vendor.Name}` currently has no items in stock.");
            return;
        }

        var embed = new DiscordEmbedBuilder
        {
            Title = $"{vendor.Name}'s Inventory",
            Description = vendor.Description,
            Color = DiscordColor.Green
        };

        foreach (var item in vendor.Inventory)
        {
            embed.AddField(
                $"{item.ItemName} x{item.Quantity}",
                $"Rarity: `{item.Rarity}` | Price: `{EconomyUtils.FormatCurrency(vendor.GetPrice(item))}`",
                inline: true
            );
        }

        await ctx.RespondAsync(embed: embed.Build());
*/

public sealed class EconomyVendorModule : BaseCommandModule
{
    [Command("vendor")]
    [Aliases("getvendor")]
    [Description("Displays the inventory of a specified vendor")]
    public async Task VendorAsync(CommandContext context, [Description("Name of the vendor"), RemainingText] string vendorName = "Gregory House")
    {
        vendorName = vendorName.ToLower();
        HouseEconomyVendor? vendor = VendorManager.Find(vendorName);

        if (vendor == null)
        {
            await context.RespondAsync($"Vendor `{vendorName}` not found");
            return;
        }

        if (vendor.Inventory.Count == 0)
        {
            await context.RespondAsync($"`{vendor.Name}` currently has no items in stock");
            return;
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = vendor.Name,
            Description = vendor.Description
        };

        if (vendor.Aliases?.Count > 0)
        {
            embedBuilder.AddField("Aliases", string.Join(", ", vendor.Aliases));
        }

        if (!string.IsNullOrWhiteSpace(vendor.Quirk))
        {
            embedBuilder.WithFooter($"{vendor.Name}'s quirk is: '{vendor.Quirk}'...", context.Client.CurrentUser.AvatarUrl);
        }

        var timeSinceLastRestock = DateTime.UtcNow - vendor.LastRestockTime;

        embedBuilder.AddField("Restock Interval", $"`{vendor.RestockInterval.TotalMinutes} minutes`");
        embedBuilder.AddField("Time Since Last Restock", $"`{timeSinceLastRestock.TotalMinutes:F1} minutes`");

        if (vendor.Inventory.Count > 0)
        {
            StringBuilder inventoryDescription = new();

            foreach (var item in vendor.Inventory.OrderByDescending(item => item.ItemName))
            {
                inventoryDescription.AppendLine($"• `{item.ItemName} ({item.Rarity})` — Price: `{EconomyUtils.FormatCurrency(vendor.GetPrice(item))}` tokens — Qty: `{item.Quantity}`");
            }

            embedBuilder.AddField("Inventory", inventoryDescription.ToString());
        }

        await context.RespondAsync(embedBuilder);
    }

    [Command("vendors")]
    [Aliases("listvendors")]
    [Description("Lists vendors and who they are!")]
    public async Task GetVendorsAsync(CommandContext context)
    {
        var pages = VendorPresets.VendorPool.Select(vendor =>
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = vendor.Name,
                Description = vendor.Description
            };

            if (vendor.Aliases?.Count > 0)
            {
                embedBuilder.AddField("Aliases", string.Join(", ", vendor.Aliases));
            }

            if (!string.IsNullOrWhiteSpace(vendor.Quirk))
            {
                embedBuilder.WithFooter($"{vendor.Name}'s quirk is: '{vendor.Quirk}'...", context.Client.CurrentUser.AvatarUrl);
            }

            var timeSinceLastRestock = DateTime.UtcNow - vendor.LastRestockTime;

            embedBuilder.AddField("Restock Interval", $"`{vendor.RestockInterval.TotalMinutes} minutes`");
            embedBuilder.AddField("Time Since Last Restock", $"`{timeSinceLastRestock.TotalMinutes:F1} minutes`");

            if (vendor.Inventory.Count > 0)
            {
                StringBuilder inventoryDescription = new();

                foreach (var item in vendor.Inventory.OrderByDescending(item => item.ItemName))
                {
                    inventoryDescription.AppendLine($"• `{item.ItemName} ({item.Rarity})` — Price: `{EconomyUtils.FormatCurrency(vendor.GetPrice(item))}` tokens — Qty: `{item.Quantity}`");
                }

                embedBuilder.AddField("Inventory", inventoryDescription.ToString());
            }

            return new Page(embed: embedBuilder);
        });

        var interactivity = context.Client.GetInteractivity();

        await interactivity.SendPaginatedMessageAsync(
            context.Channel,
            context.User,
            pages,
            TimeSpan.FromMinutes(2),
            DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
            DSharpPlus.Interactivity.Enums.ButtonPaginationBehavior.DeleteMessage
        );
    }

    [Command("buy")]
    [Aliases("purchase", "buyitem", "purchaseitem")]
    [Description("Purchases an item from a vendor")]
    public async Task BuyItemAsync(CommandContext context, [Description("Name of the item"), RemainingText] string itemName)
    {

    }

    [Command("items")]
    [Aliases("listitems", "getitems")]
    [Description("Lists ALL items available in House economy")]
    public async Task GetItemsAsync(CommandContext context)
    {
        var pages = GlobalItemPool.AllItems.Select(item =>
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = item.ItemName,
                Description = item.Description
            };

            return new Page(embed: embedBuilder);
        });

        var interactivity = context.Client.GetInteractivity();

        await interactivity.SendPaginatedMessageAsync(
            context.Channel,
            context.User,
            pages,
            TimeSpan.FromMinutes(2),
            DSharpPlus.Interactivity.Enums.PaginationBehaviour.Ignore,
            DSharpPlus.Interactivity.Enums.ButtonPaginationBehavior.DeleteMessage
        );
    }

    [Command("item")]
    [Aliases("viewitem", "getitem")]
    [Description("Displays the details of a particular item")]
    public async Task GetItemAsync(CommandContext context, [Description("Name of the item"), RemainingText] string itemName)
    {
        itemName = itemName.ToLower();
        HouseEconomyItem? item = ItemManager.Find(itemName);

        if (item == null)
        {
            await context.RespondAsync($"`{itemName}` does not exist");
            return;
        }

        DiscordEmbedBuilder embedBuilder = new()
        {
            Title = item.ItemName,
            Description = item.Description,
            Color = item.Rarity.GetDiscordColor()
        };

        embedBuilder.AddField("Rarity", $"{item.Rarity.GetEmoji()} - {item.Rarity.GetDisplayName()}");
        embedBuilder.AddField("Stackable", item.IsStackable ? "yes" : "no");
        embedBuilder.AddField("Purchasable", item.IsPurchaseable ? "yes" : "no");
        embedBuilder.AddField("Value", EconomyUtils.FormatCurrency(item.Value));

        await context.RespondAsync(embedBuilder);
    }
}