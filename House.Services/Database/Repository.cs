using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Protection;
using MongoDB.Driver;

namespace House.House.Services.Database;

public static class RepositoryExceptionHelper
{
    public static TException ThrowEntityNotFound<TException>(ulong ID) where TException : Exception
    {
        var constructor = typeof(TException).GetConstructor([typeof(ulong)])
            ?? throw new InvalidOperationException($"Exception type {typeof(TException).Name} could not be found/must have a constructor accepting a ulong type");

        throw (TException)constructor.Invoke([ID]);
    }

    public static TException ThrowEntityExists<TException>(ulong ID) where TException : Exception
    {
        var constructor = typeof(TException).GetConstructor([typeof(ulong)])
            ?? throw new InvalidOperationException($"Exception type {typeof(TException).Name} could not be found/must have a constructor accepting a ulong type");

        throw (TException)constructor.Invoke([ID]);
    }
}

public abstract class Repository<T> where T : DatabaseEntity
{
    protected readonly IMongoCollection<T> Collection;
    protected abstract string CollectionName { get; }

    protected Repository(IMongoDatabase database)
    {
        Collection = database.GetCollection<T>(CollectionName);
    }

    public async Task<T> GetAsync(ulong ID)
    {
        return await Collection.Find(u => u.ID == ID).FirstOrDefaultAsync() ?? throw new EntityNotFoundException(ID);
    }

    public async Task<T?> TryGetAsync(ulong ID)
    {
        return await Collection.Find(u => u.ID == ID).FirstOrDefaultAsync();
    }

    public async Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var existing = await Collection.Find(e => e.ID == entity.ID).FirstOrDefaultAsync();
        if (existing != null)
        {
            throw new EntityExistsException($"An entity with ID '{entity.ID}' already exists");
        }

        await Collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var result = await Collection.ReplaceOneAsync(e => e.ID == entity.ID, entity);
        if (result.MatchedCount == 0)
        {
            throw new EntityNotFoundException($"Entity with ID '{entity.ID}' not found");
        }
    }

    public async Task DeleteAsync(ulong ID)
    {
        var result = await Collection.DeleteOneAsync(e => e.ID == ID);
        if (result.DeletedCount == 0)
        {
            throw new EntityNotFoundException($"Entity with ID '{ID}' not found.");
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}

public abstract class ValidatedRepositoryBase<TEntity, TNotFound, TExists> : Repository<TEntity>
    where TEntity : DatabaseEntity
    where TNotFound : Exception
    where TExists : Exception
{
    protected ValidatedRepositoryBase(IMongoDatabase database) : base(database) { }

    public new async Task<TEntity> GetAsync(ulong id)
    {
        var entity = await TryGetAsync(id);
        if (entity is null)
        {
            RepositoryExceptionHelper.ThrowEntityNotFound<TNotFound>(id);
        }

        return entity!;
    }

    public new async Task AddAsync(TEntity entity)
    {
        var existing = await TryGetAsync(entity.ID);
        if (existing is not null)
        {
            RepositoryExceptionHelper.ThrowEntityExists<TExists>(entity.ID);
        }

        await base.AddAsync(entity);
    }

    public async Task<TEntity> GetOrCreateAsync(ulong ID, Func<ulong, Task<TEntity>> createEntityAsync, Func<TEntity, Task<TEntity>>? updateExistingAsync = null)
    {
        ArgumentNullException.ThrowIfNull(createEntityAsync);

        var existing = await TryGetAsync(ID);
        if (existing is not null)
        {
            if (updateExistingAsync is not null)
            {
                var updated = await updateExistingAsync(existing);
                ArgumentNullException.ThrowIfNull(updated);

                await UpdateAsync(updated);
                return updated;
            }

            return existing;
        }

        var newEntity = await createEntityAsync(ID);

        ArgumentNullException.ThrowIfNull(newEntity);

        await AddAsync(newEntity);
        return newEntity;
    }
}

public sealed class WhitelistedUserRepository :
    ValidatedRepositoryBase<WhitelistedUser, WhitelistedUserNotFoundException, WhitelistedUserExistsException>
{
    protected override string CollectionName => "whitelisted_users";
    public WhitelistedUserRepository(IMongoDatabase database) : base(database) { }
}

public sealed class BlacklistedUserRepository :
    ValidatedRepositoryBase<BlacklistedUser, BlacklistedUserNotFoundException, BlacklistedUserExistsException>
{
    protected override string CollectionName => "blacklisted_users";
    public BlacklistedUserRepository(IMongoDatabase database) : base(database) { }
}

public sealed class StaffUserRepository :
    ValidatedRepositoryBase<StaffUser, StaffUserNotFoundException, StaffUserExistsException>
{
    protected override string CollectionName => "staff_users";
    public StaffUserRepository(IMongoDatabase database) : base(database) { }
}

public sealed class SuspectMemberRepository :
    ValidatedRepositoryBase<SuspectMember, SuspectMemberNotFoundException, SuspectMemberExistsException>
{
    protected override string CollectionName => "flagged_members";
    public SuspectMemberRepository(IMongoDatabase database) : base(database) { }
}

public sealed class GuildRepository : ValidatedRepositoryBase<DatabaseGuild, GuildNotFoundException, GuildExistsException>
{
    protected override string CollectionName => "guilds";

    public GuildRepository(IMongoDatabase database) : base(database) { }
}

public sealed class StarboardRepository : ValidatedRepositoryBase<StarboardEntry, StarboardEntryNotFoundException, StarboardEntryExistsException>
{
    protected override string CollectionName => "starboard_entries";

    public StarboardRepository(IMongoDatabase database) : base(database)
    {
    }

    public new async Task<StarboardEntry?> TryGetAsync(ulong messageId)
    {
        return await Collection.Find(e => e.MessageID == messageId).FirstOrDefaultAsync();
    }

    public new async Task<StarboardEntry> GetAsync(ulong messageId)
    {
        var entry = await TryGetAsync(messageId);
        if (entry == null)
        {
            RepositoryExceptionHelper.ThrowEntityNotFound<StarboardEntryNotFoundException>(messageId);
        }

        return entry!;
    }

    public new async Task DeleteAsync(ulong messageId)
    {
        var result = await Collection.DeleteOneAsync(e => e.MessageID == messageId);
        if (result.DeletedCount == 0)
        {
            RepositoryExceptionHelper.ThrowEntityNotFound<StarboardEntryNotFoundException>(messageId);
        }
    }
}

public sealed class BackupRepository : ValidatedRepositoryBase<BackupGuild, GuildNotFoundException, GuildExistsException>
{
    protected override string CollectionName => "guild_backups";

    public BackupRepository(IMongoDatabase database) : base(database)
    {
    }
}

public sealed class SnipeRepository : ValidatedRepositoryBase<SnipedMessage, SnipedMessageNotFoundException, SnipedMessageExistsException>
{
    protected override string CollectionName => "sniped_messages";

    public SnipeRepository(IMongoDatabase database) : base(database)
    {

    }

    public async Task<SnipedMessage?> GetLastMessageAsync(ulong channelId)
    {
        var filter = Builders<SnipedMessage>.Filter.Eq(x => x.ChannelID, channelId);
        var cursor = Collection.Find(filter);

        return await cursor.FirstOrDefaultAsync();
    }
}

/*
public abstract class Repository<T, TKey> where T : DatabaseEntity<TKey>
{
    protected readonly IMongoCollection<T> Collection;

    protected abstract string CollectionName { get; }

    protected Repository(IMongoDatabase database)
    {
        Collection = database.GetCollection<T>(CollectionName);
    }

    public async Task<T> GetAsync(string ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            throw new ArgumentException("Entity ID cannot be empty");
        }

        var user = await Collection.Find(u => u.ID == ID).FirstOrDefaultAsync() ?? throw new ObjectNotFoundException(ID);
        return user;
    }

    public async Task<T?> TryGetAsync(string ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            throw new ArgumentException("Entity ID cannot be empty");
        }

        return await Collection.Find(u => u.ID == ID).FirstOrDefaultAsync();
    }

    public async Task AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (string.IsNullOrWhiteSpace(entity.ID))
        {
            throw new ArgumentException("Entity ID cannot be empty");
        }

        var existing = await Collection.Find(e => e.ID == entity.ID).FirstOrDefaultAsync();
        if (existing != null)
        {
            throw new ObjectAlreadyExistsException($"An entity with ID '{entity.ID}' already exists");
        }

        await Collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentException("Entity cannot be null");
        }

        if (string.IsNullOrWhiteSpace(entity.ID))
        {
            throw new ArgumentException("Entity ID cannot be empty");
        }

        var result = await Collection.ReplaceOneAsync(e => e.ID == entity.ID, entity);
        if (result.MatchedCount == 0)
        {
            throw new ObjectNotFoundException($"Entity with ID '{entity.ID}' not found");
        }
    }

    public async Task DeleteAsync(string ID)
    {
        if (string.IsNullOrWhiteSpace(ID))
        {
            throw new ArgumentException("ID cannot be empty", nameof(ID));
        }

        var result = await Collection.DeleteOneAsync(e => e.ID == ID);
        if (result.DeletedCount == 0)
        {
            throw new ObjectNotFoundException($"Entity with ID '{ID}' not found.");
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }
}

public class WhitelistedUserRepository : Repository<WhitelistedUser>
{
    protected override string CollectionName => "whitelisted_users";

    public WhitelistedUserRepository(IMongoDatabase database) : base(database) { }
}

public class BlacklistedUserRepository : Repository<BlacklistedUser>
{
    protected override string CollectionName => "blacklisted_users";

    public BlacklistedUserRepository(IMongoDatabase database) : base(database) { }
}

public class StaffUserRepository : Repository<StaffUser>
{
    protected override string CollectionName => "staff_users";

    public StaffUserRepository(IMongoDatabase database) : base(database)
    {

    }
}

public class GuildRepository : Repository<DatabaseGuild>
{
    protected override string CollectionName => "guilds";

    public GuildRepository(IMongoDatabase database) : base(database) { }
}

public class StarboardRepository : Repository<StarboardEntry>
{
    protected override string CollectionName => "starboard_entries";

    public StarboardRepository(IMongoDatabase database) : base(database)
    {

    }
}
*/