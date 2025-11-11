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
}