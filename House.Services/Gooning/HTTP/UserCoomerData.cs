using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using House.House.Services.Database;

namespace House.House.Services.Gooning.HTTP;

public class NotificationPreferences
{
    public bool Enabled { get; set; } = true;

    public TimeSpan? NotifyCooldown { get; set; }

    public List<string> NotifyOnServices { get; set; } = [];
    public List<string> NotifyOnUsers { get; set; } = [];
}

public class PostCollection
{
    public string Name { get; set; } = string.Empty;
    public List<string> PostIDs { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class TrackedCreator
{
    public string Service { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public string? LastPostIdSeen { get; set; }
    public bool NotifyOnNewPost { get; set; } = false;
}

public class ActionLogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = string.Empty;
    public string? Target { get; set; }
}

public class CustomPostMetadata
{
    public string PostId { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public int? Rating { get; set; }
    public string? Notes { get; set; }
}

public class UserCoomerData : DatabaseEntity
{
    public Dictionary<string, bool> Settings { get; set; } = [];

    public NotificationPreferences Notifications { get; set; } = new();

    public List<string> SavedPostIDs { get; set; } = [];
    public List<string> RecentPostIDs { get; set; } = [];
    public List<string> HiddenCreatorIDs { get; set; } = [];
    public List<string> HiddenPostIDs { get; set; } = [];

    public List<TrackedCreator> TrackedCreators { get; set; } = [];
    public List<PostCollection> Collections { get; set; } = [];
    public List<CustomPostMetadata> PostMetadata { get; set; } = [];
    public List<ActionLogEntry> ActivityLog { get; set; } = [];

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public int SchemaVersion { get; set; } = 1;

    public void AddTrackedCreator(string service, string username, bool notifyOnNewPost = false)
    {
        string normalizedService = service.Trim();
        string normalizedUsername = service.Trim();

        if (!TrackedCreators.Any(tc => tc.Service.Equals(normalizedService, StringComparison.OrdinalIgnoreCase) && tc.Username.Equals(normalizedUsername, StringComparison.OrdinalIgnoreCase)))
        {
            TrackedCreator creator = new()
            {
                Service = service,
                Username = username,
                NotifyOnNewPost = notifyOnNewPost,
                SubscribedAt = DateTime.UtcNow
            };

            TrackedCreators.Add(creator);
            LogAction($"Subscribed to {service}/{username}");

            LastUpdated = DateTime.UtcNow;
        }
    }

    public void RemoveTrackedCreator(string service, string username)
    {
        TrackedCreator? creator = TrackedCreators.FirstOrDefault(tc => tc.Service.Equals(service,
            StringComparison.OrdinalIgnoreCase) && tc.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (creator != null)
        {
            TrackedCreators.Remove(creator);
            LogAction($"Unsubscribed from {service}/{username}");

            LastUpdated = DateTime.UtcNow;
        }
    }

    public void SavePost(string postID)
    {
        if (!SavedPostIDs.Contains(postID))
        {
            SavedPostIDs.Add(postID);
            LogAction($"Saved post {postID}");

            LastUpdated = DateTime.UtcNow;
        }
    }

    public void RemoveSavedPost(string postID)
    {
        if (SavedPostIDs.Remove(postID))
        {
            LogAction($"Removed saved post {postID}");
            LastUpdated = DateTime.UtcNow;
        }
    }

    public void LogAction(string action, string? target = null)
    {
        if (ActivityLog.Count >= 1000)
        {
            ActivityLog.RemoveAt(0);
        }

        ActionLogEntry entry = new()
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            Target = target
        };

        ActivityLog.Add(entry);
    }

    public void UpdateNotificationPreferences(bool enabled, TimeSpan? cooldown = null, IEnumerable<string>? services = null, IEnumerable<string>? users = null)
    {
        Notifications.Enabled = enabled;
        Notifications.NotifyCooldown = cooldown;

        if (services != null)
        {
            Notifications.NotifyOnUsers = services.Select(s => s.Trim()).ToList();
        }

        if (users != null)
        {
            Notifications.NotifyOnUsers = users.Select(u => u.Trim()).ToList();
        }

        LogAction("Updated notification preferences");
        LastUpdated = DateTime.UtcNow;
    }

    public TrackedCreator? GetTrackedCreator(string service, string username)
    {
        return TrackedCreators.FirstOrDefault(tc =>
            tc.Service.Equals(service, StringComparison.OrdinalIgnoreCase) &&
            tc.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }
}