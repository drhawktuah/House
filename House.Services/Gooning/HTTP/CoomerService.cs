using House.House.Services.Gooning.Exceptions;

namespace House.House.Services.Gooning.HTTP;

public sealed record CoomerService(string Name, string ApiSlug)
{
    public static readonly CoomerService OnlyFans = new("OnlyFans", "onlyfans");
    public static readonly CoomerService Fansly = new("Fansly", "fansly");
    public static readonly CoomerService Patreon = new("Patreon", "patreon");

    public static readonly IReadOnlyList<CoomerService> services = [OnlyFans, Fansly, Patreon];

    public static CoomerService FromSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Slug cannot be null or empty", nameof(slug));
        }

        CoomerService? service = services.FirstOrDefault(s => s.ApiSlug.Equals(slug, StringComparison.OrdinalIgnoreCase))
            ?? throw new CoomerServiceException($"{slug} is not a valid service!");

        return service;
    }

    public static bool TryFromSlug(string slug, out CoomerService? service)
    {
        service = null;

        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        service = services.FirstOrDefault(s => s.ApiSlug.Equals(slug, StringComparison.OrdinalIgnoreCase));

        return service is not null;
    }
}
