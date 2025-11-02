using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Gooning.HTTP;

public record Endpoints
{
    public string BaseURL { get; }

    public string Creators => $"{BaseURL}/api/v1/creators";

    public Endpoints(string baseURL)
    {
        if (string.IsNullOrWhiteSpace(baseURL))
        {
            throw new ArgumentException("Base url cannot be null or empty", nameof(baseURL));
        }

        BaseURL = baseURL.TrimEnd('/');
    }

    public string Creator(string service, string username)
    {
        return $"{BaseURL}/api/v1/{service}/user/{Uri.EscapeDataString(username)}/profile";
    }

    public string Posts(string service, string username, int? limit = null, int? offset = null)
    {
        var url = $"{BaseURL}/api/v1/{Uri.EscapeDataString(service)}/user/{Uri.EscapeDataString(username)}/posts";

        List<string> queryParams = [];

        if (limit.HasValue)
        {
            queryParams.Add($"limit={limit.Value}");
        }

        if (offset.HasValue)
        {
            queryParams.Add($"offset={offset.Value}");
        }

        return queryParams.Count > 0 ? $"{url}?{string.Join('&', queryParams)}" : url;
    }

    public string PostById(long postId)
    {
        return $"{BaseURL}/api/v1/post/{postId}";
    }
}