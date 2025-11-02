using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Gooning.HTTP;

public class CoomerCreator
{
    public string ID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;

    public string ProfileUrl => $"https://coomer.st/{Service}/{ID}";
    public string ImageUrl => $"https://img.coomer.st/icons/{Service.ToLowerInvariant()}/{ID}";

    public long Favorited { get; set; }

    public int PostCount { get; set; }
    public int DMCount { get; set; }
    public int ShareCount { get; set; }
    public int ChatCount { get; set; }

    public string FormattedFavoritedCounbt => Favorited.ToString("N0");
    public string FormattedPostCount => PostCount.ToString("N0");
    public string FormattedDMCount => DMCount.ToString("N0");
    public string FormattedShareCount => ShareCount.ToString("N0");
    public string FormattedChatCount => ChatCount.ToString("N0");

    public long Indexed { get; set; }
    public long Updated { get; set; }

    public DateTime IndexedDateTime => DateTimeOffset.FromUnixTimeSeconds(Indexed).UtcDateTime;
    public DateTime UpdatedDateTime => DateTimeOffset.FromUnixTimeSeconds(Updated).UtcDateTime;


    public IReadOnlyList<CoomerPost> Posts { get; set; } = [];
}