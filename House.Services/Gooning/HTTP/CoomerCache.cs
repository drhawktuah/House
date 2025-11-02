using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Gooning.HTTP;

internal sealed class CachedCreator
{
    public CoomerCreator Creator { get; }
    public DateTime CachedAt { get; }

    public CachedCreator(CoomerCreator creator)
    {
        Creator = creator;
        CachedAt = DateTime.UtcNow;
    }
}

public class CoomerCache : IDisposable
{
    private readonly ConcurrentDictionary<string, CachedCreator> creatorCache = [];

    private readonly TimeSpan cacheDuration;
    private readonly Timer cleanupTimer;

    private int cleanupRunning = 0;

    public CoomerCache(TimeSpan? cacheDuration = null, TimeSpan? cleanupInterval = null)
    {
        this.cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(10);
        var interval = cleanupInterval ?? TimeSpan.FromMinutes(5);

        cleanupTimer = new Timer(CleanupTimerCallback, null, interval, interval);
    }

    public bool TryGetCreator(string service, string username, out CoomerCreator? creator)
    {
        var key = NormalizeKey(service, username);

        if (creatorCache.TryGetValue(key, out var cached))
        {
            if (DateTime.UtcNow - cached.CachedAt < cacheDuration)
            {
                creator = cached.Creator;
                return true;
            }
            else
            {
                creatorCache.TryRemove(key, out _);
            }
        }

        creator = null;
        return false;
    }

    public void AddOrUpdateCreator(string service, string username, CoomerCreator creator)
    {
        var key = NormalizeKey(service, username);
        var cached = new CachedCreator(creator);

        creatorCache.AddOrUpdate(key, cached, (k, existing) => cached);
    }

    public void ClearExpired()
    {
        var now = DateTime.UtcNow;

        foreach (var (username, creator) in creatorCache)
        {
            if (now - creator.CachedAt >= cacheDuration)
            {
                creatorCache.TryRemove(username, out _);
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        cleanupTimer.Dispose();
    }

    private void CleanupTimerCallback(object? state)
    {
        if (Interlocked.Exchange(ref cleanupRunning, 1) == 1)
        {
            return;
        }

        try
        {
            ClearExpired();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup exception: {ex}");
        }
        finally
        {
            Interlocked.Exchange(ref cleanupRunning, 0);
        }
    }

    private static string NormalizeKey(string service, string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        service = service.Trim().ToLowerInvariant();
        username = username.Trim().ToLowerInvariant();

        return $"{service}:{username}";
    }
}


public class UserDataCache : IDisposable
{
    private readonly EncryptedStorageService<UserCoomerData> storageService;
    private readonly Timer saveTimer;
    private readonly TimeSpan saveInterval;

    private readonly ConcurrentDictionary<ulong, UserCoomerData> dataCache = [];
    private readonly ConcurrentDictionary<ulong, bool> dirtyCache = [];
    private readonly ConcurrentDictionary<ulong, string> passwordCache = [];

    private bool isSaving = false;
    private readonly Lock saveLock = new();

    private bool disposed = false;

    public UserDataCache(EncryptedStorageService<UserCoomerData> storageService)
    {
        this.storageService = storageService;

        saveInterval = TimeSpan.FromMinutes(5);
        saveTimer = new Timer(_ => AutoSaveCallbackAsync().GetAwaiter().GetResult(), null, saveInterval, saveInterval);
    }

    public void SetUserData(ulong userId, UserCoomerData data)
    {
        dataCache[userId] = data;
        dirtyCache[userId] = true;
    }

    public bool TryGetUserData(ulong userId, out UserCoomerData? data)
    {
        return dataCache.TryGetValue(userId, out data);
    }

    public void CacheUserPassword(ulong userId, string password)
    {
        passwordCache[userId] = password;
    }

    public void RemoveCachedPassword(ulong userId)
    {
        passwordCache.TryRemove(userId, out _);
    }

    public async Task<bool> TryLoadUserDataAsync(ulong ID, string password)
    {
        var data = await storageService.LoadDecryptedAsync(ID, password);
        if (data != null)
        {
            dataCache[ID] = data;
            passwordCache[ID] = password;
            dirtyCache[ID] = false;

            return true;
        }

        return false;
    }

    public async Task SaveNowAsync(Func<ulong, string> passwordProvider)
    {
        lock (saveLock)
        {
            if (isSaving)
            {
                return;
            }

            isSaving = true;
        }

        try
        {
            foreach (var userId in dirtyCache.Keys)
            {
                if (!dirtyCache.TryGetValue(userId, out var isDirty) || !isDirty)
                {
                    continue;
                }

                if (!dataCache.TryGetValue(userId, out var userData))
                {
                    continue;
                }

                string password;

                try
                {
                    password = passwordProvider(userId);
                }
                catch
                {
                    continue;
                }

                try
                {
                    await storageService.StoreEncryptedAsync(userId, userData, password);
                    dirtyCache[userId] = false;

                    passwordCache.TryRemove(userId, out _);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SaveNow] Error saving user {userId}: {ex.Message}");
                }
            }
        }
        finally
        {
            lock (saveLock)
            {
                isSaving = false;
            }
        }
    }

    public async Task SaveAllOnShutdownAsync()
    {
        await SaveNowAsync(userID =>
        {
            if (passwordCache.TryGetValue(userID, out var password))
            {
                return password;
            }

            throw new InvalidOperationException("Password not cached on shutdown");
        });
    }

    private async Task AutoSaveCallbackAsync()
    {
        try
        {
            await SaveNowAsync(userID =>
            {
                if (passwordCache.TryGetValue(userID, out var password))
                {
                    return password;
                }

                throw new InvalidOperationException("Password not cached");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AutoSave] Error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        saveTimer.Dispose();
        disposed = true;

        GC.SuppressFinalize(this);
    }

    ~UserDataCache()
    {
        Dispose();
    }
}
