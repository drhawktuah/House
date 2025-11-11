using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Database;
using House.House.Services.Gooning.HTTP;
using MongoDB.Driver;

namespace House.House.Services.Gooning;

public class GooningService : Repository<UserCoomerData>
{
    protected override string CollectionName => "usercoomerdata";

    public GooningService(IMongoDatabase database) : base(database)
    {

    }

    public async Task<List<UserCoomerData>> GetLeaderboardAsync(SortDefinition<UserCoomerData> sort, int page = 0, int pageSize = 10)
    {
        return await Collection.Find(_ => true)
            .Sort(sort)
            .Skip(page * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> GetUserCountAsync(FilterDefinition<UserCoomerData>? filter = null)
    {
        filter ??= Builders<UserCoomerData>.Filter.Empty;

        return await Collection.CountDocumentsAsync(_ => true);
    }
}