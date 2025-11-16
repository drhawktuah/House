using House.House.Services.Economy;
using House.House.Services.Economy.General;
using House.House.Services.Economy.Vendors;
using MongoDB.Driver;

namespace House.House.Services.Economy;

public class HouseEconomyDatabase
{
    private readonly IMongoCollection<HouseEconomyUser> collection;

    public HouseEconomyDatabase(IMongoClient client, string databaseName = "EconomyDB", string collectionName = "Users")
    {
        var database = client.GetDatabase(databaseName);

        collection = database.GetCollection<HouseEconomyUser>(collectionName);
    }

    public async Task<HouseEconomyUser> GetUserAsync(ulong userId)
    {
        var user = await collection.Find(u => u.ID == userId).FirstOrDefaultAsync() ?? throw new UserNotFoundException(userId);

        return user;
    }

    public async Task<HouseEconomyUser?> TryGetPlayerAsync(ulong ID)
    {
        return await collection.Find(u => u.ID == ID).FirstOrDefaultAsync();
    }

    public async Task<List<HouseEconomyUser>> GetAllUsersAsync()
    {
        return await collection.Find(_ => true).ToListAsync();
    }

    public async Task CreateUserAsync(ulong userId)
    {
        var existingUser = await collection.Find(u => u.ID == userId)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            throw new UserAlreadyExistsException(userId);
        }

        var newUser = new HouseEconomyUser { ID = userId };
        await collection.InsertOneAsync(newUser);
    }

    public async Task UpdateUserAsync(HouseEconomyUser user)
    {
        var result = await collection.ReplaceOneAsync(
            Builders<HouseEconomyUser>.Filter.Eq(u => u.ID, user.ID),
            user,
            new ReplaceOptions
            {
                IsUpsert = false
            }
        );

        if (result.MatchedCount == 0)
        {
            throw new UserNotFoundException(user.ID);
        }
    }

    public async Task ChangeBalanceAsync(ulong userId, long? bank = null, long? cash = null)
    {
        var updates = new List<UpdateDefinition<HouseEconomyUser>>();

        if (bank.HasValue)
        {
            updates.Add(Builders<HouseEconomyUser>.Update.Set(u => u.Bank, bank.Value));
        }

        if (cash.HasValue)
        {
            updates.Add(Builders<HouseEconomyUser>.Update.Set(u => u.Cash, cash.Value));
        }

        if (updates.Count == 0)
        {
            throw new NoBalanceChangeProvidedException();
        }

        var combinedUpdate = Builders<HouseEconomyUser>.Update.Combine(updates);

        var result = await collection.UpdateOneAsync(
            Builders<HouseEconomyUser>.Filter.Eq(u => u.ID, userId),
            combinedUpdate);

        if (result.MatchedCount == 0)
        {
            throw new UserNotFoundException(userId);
        }
    }

    public async Task ModifyBalanceAsync(ulong userId, long bank = 0, long cash = 0)
    {
        var updates = new List<UpdateDefinition<HouseEconomyUser>>();

        if (bank != 0)
        {
            updates.Add(Builders<HouseEconomyUser>.Update.Inc(u => u.Bank, bank));
        }

        if (cash != 0)
        {
            updates.Add(Builders<HouseEconomyUser>.Update.Inc(u => u.Cash, cash));
        }

        if (updates.Count == 0)
        {
            throw new NoBalanceChangeProvidedException();
        }

        var update = Builders<HouseEconomyUser>.Update.Combine(updates);

        var result = await collection.UpdateOneAsync(
            Builders<HouseEconomyUser>.Filter.Eq(u => u.ID, userId),
            update
        );

        if (result.MatchedCount == 0)
        {
            throw new UserNotFoundException(userId);
        }
    }

    public async Task AddItemAsync(ulong userID, HouseEconomyItem item)
    {
        var update = Builders<HouseEconomyUser>.Update.Push(u => u.Inventory, item);
        var result = await collection.UpdateOneAsync(u => u.ID == userID, update);

        if (result.MatchedCount == 0)
        {
            throw new UserNotFoundException(userID);
        }
    }

    public async Task RemoveItemAsync(ulong userId, string itemName, int quantity = 1)
    {
        var user = await GetUserAsync(userId);
        var inventoryItem = user.Inventory.FirstOrDefault(i => i.ItemName == itemName);

        if (inventoryItem == null)
        {
            return;
        }

        if (inventoryItem.IsStackable)
        {
            inventoryItem.Quantity -= quantity;

            if (inventoryItem.Quantity <= 0)
            {
                user.Inventory.Remove(inventoryItem);
            }
        }
        else
        {
            user.Inventory.Remove(inventoryItem);
        }

        await UpdateUserAsync(user);
    }

    public async Task<PurchaseResult> BuyItemAsync(ulong userID, HouseEconomyVendor vendor, string itemName, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName) || quantity <= 0)
        {
            return PurchaseResult.InvalidQuantity;
        }

        var user = await GetUserAsync(userID);
        var item = vendor.Inventory.FirstOrDefault(i => i.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            return PurchaseResult.ItemNotFound;
        }

        if (!item.IsPurchaseable)
        {
            return PurchaseResult.NotPurchasable;
        }

        long itemPrice = vendor.GetPrice(item);
        long totalPrice = itemPrice * quantity;

        if (user.Cash < totalPrice)
        {
            return PurchaseResult.NotEnoughCash;
        }

        user.Cash -= totalPrice;

        var purchased = item.CloneWithQuantity(quantity);

        var existingItem = user.Inventory.FirstOrDefault(i => i.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        if (existingItem != null && existingItem.IsStackable)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            user.Inventory.Add(purchased);
        }

        await UpdateUserAsync(user);

        return PurchaseResult.Success;
    }

    public async Task<PurchaseResult> SellItemAsync(ulong userID, HouseEconomyVendor vendor, string itemName, int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(itemName) || quantity <= 0)
        {
            return PurchaseResult.InvalidInput;
        }

        var user = await GetUserAsync(userID);
        var item = user.Inventory.FirstOrDefault(i => i.ItemName.Equals(itemName, StringComparison.OrdinalIgnoreCase));

        if (item == null)
        {
            return PurchaseResult.ItemNotFound;
        }

        long sellPrice = (long)(vendor.GetPrice(item) * 0.5) * quantity;

        await RemoveItemAsync(userID, item.ItemName, quantity);

        user.Cash += sellPrice;

        await UpdateUserAsync(user);

        return PurchaseResult.Success;
    }
}