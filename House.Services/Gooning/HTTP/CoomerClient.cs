using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using House.House.Services.Gooning.Exceptions;
using Newtonsoft.Json.Converters;

namespace House.House.Services.Gooning.HTTP;

public interface ICoomerClient
{
    IReadOnlyList<CoomerService> Services { get; }

    Task<CoomerCreator> GetCreatorAsync(string service, string username);
    Task<CoomerPost> GetPostAsync(long postID);
    Task<CoomerPost> GetLatestPostAsync(string service, string username);
    Task<IReadOnlyList<CoomerPost>> GetPostsAsync(string service, string username, int limit = 50, int offset = 0);
    Task<IReadOnlyList<CoomerCreator>> GetCreatorsAsync();
}

public sealed class CoomerClient : ICoomerClient
{
    public IReadOnlyList<CoomerService> Services => CoomerService.services;
    private readonly HttpClient client;
    private readonly CoomerCache coomerCache;
    private readonly UserDataCache userDataCache;
    private readonly Endpoints endpoints;

    private static readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CoomerClient(HttpClient client, UserDataCache userDataCache, CoomerCache? coomerCache = null, Endpoints? endpoints = null)
    {
        this.client = client;
        this.userDataCache = userDataCache;
        this.coomerCache = coomerCache ?? new CoomerCache();
        this.endpoints = endpoints ?? new Endpoints("https://coomer.st");
    }

    public async Task<CoomerCreator> GetCreatorAsync(string service, string username)
    {
        if (coomerCache.TryGetCreator(service, username, out var creator) && creator is not null)
        {
            return creator;
        }

        var url = endpoints.Creator(service, username);

        HttpResponseMessage message;
        string json;

        try
        {
            message = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex);

            throw new CoomerHTTPException(HttpStatusCode.ServiceUnavailable, url, null, "Failed to reach the server");
        }

        if (message.StatusCode == HttpStatusCode.NotFound)
        {
            throw new CoomerCreatorNotFoundException(service, username);
        }

        if (!message.IsSuccessStatusCode)
        {
            string errorContent = await message.Content.ReadAsStringAsync();
            throw new CoomerHTTPException(message.StatusCode, url, errorContent);
        }

        try
        {
            json = await message.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            var found = JsonSerializer.Deserialize<CoomerCreator>(json, serializerOptions) ??
                throw new CoomerDeserializationException("Received null when deserializing creator", url, json);

            coomerCache.AddOrUpdateCreator(service, username, found);

            return found;
        }
        catch (JsonException ex)
        {
            throw new CoomerDeserializationException("Failed to deserialize creator response", url, null, ex);
        }
    }

    public async Task<IReadOnlyList<CoomerCreator>> GetCreatorsAsync()
    {
        var url = endpoints.Creators;

        HttpResponseMessage message;

        try
        {
            message = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex);

            throw new CoomerHTTPException(HttpStatusCode.ServiceUnavailable, url, null, "Failed to reach the server");
        }

        if (!message.IsSuccessStatusCode)
        {
            var errorContent = await message.Content.ReadAsStringAsync();
            throw new CoomerHTTPException(message.StatusCode, url, errorContent);
        }

        string json = await message.Content.ReadAsStringAsync();

        try
        {
            var creators = JsonSerializer.Deserialize<List<CoomerCreator>>(json, serializerOptions)
                ?? throw new CoomerDeserializationException("Received null when deserializing creators.", url, json);

            var sorted = creators.OrderByDescending(c => c.Favorited).ToList();

            return sorted;
        }
        catch (JsonException ex)
        {
            Console.WriteLine(json);

            Console.WriteLine(ex);

            throw new CoomerDeserializationException("Failed to deserialize creators response.", url, json, ex);
        }
    }

    public async Task<CoomerPost> GetPostAsync(long postID)
    {
        var url = endpoints.PostById(postID);

        HttpResponseMessage message;

        try
        {
            message = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex);

            throw new CoomerHTTPException(HttpStatusCode.ServiceUnavailable, url, null, "Failed to reach the server");
        }

        if (message.StatusCode == HttpStatusCode.NotFound)
        {
            throw new CoomerPostNotFoundException(postID);
        }

        if (!message.IsSuccessStatusCode)
        {
            var errorContent = await message.Content.ReadAsStringAsync();
            throw new CoomerHTTPException(message.StatusCode, url, errorContent);
        }

        string json = await message.Content.ReadAsStringAsync();

        try
        {
            var post = JsonSerializer.Deserialize<CoomerPost>(json, serializerOptions)
                ?? throw new CoomerDeserializationException("Received null when deserializing post", url, json);

            return post;
        }
        catch (JsonException ex)
        {
            throw new CoomerDeserializationException("Failed to deserialize post response", url, json, ex);
        }
    }

    public async Task<IReadOnlyList<CoomerPost>> GetPostsAsync(string service, string username, int limit = 50, int offset = 0)
    {
        var url = endpoints.Posts(service, username, limit, offset);

        HttpResponseMessage message;

        try
        {
            message = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex);

            throw new CoomerHTTPException(HttpStatusCode.ServiceUnavailable, url, null, "Failed to reach the server");
        }

        if (message.StatusCode == HttpStatusCode.NotFound)
        {
            throw new CoomerPostsNotFoundException(service, username);
        }

        if (!message.IsSuccessStatusCode)
        {
            var errorContent = await message.Content.ReadAsStringAsync();
            throw new CoomerHTTPException(message.StatusCode, url, errorContent);
        }

        string json = await message.Content.ReadAsStringAsync();

        try
        {
            var posts = JsonSerializer.Deserialize<List<CoomerPost>>(json, serializerOptions)
                ?? throw new CoomerDeserializationException("Received null when deserializing posts", url, json);

            return posts;
        }
        catch (JsonException ex)
        {
            throw new CoomerDeserializationException("Failed to deserialize posts response", url, json, ex);
        }
    }

    public async Task<CoomerPost> GetLatestPostAsync(string service, string username)
    {
        var url = endpoints.Posts(service, username, limit: 1);

        HttpResponseMessage message;

        try
        {
            message = await client.GetAsync(url);
        }
        catch (HttpRequestException)
        {
            throw new CoomerHTTPException(HttpStatusCode.ServiceUnavailable, url, null, "Failed to reach the server");
        }

        if (!message.IsSuccessStatusCode)
        {
            var errorContent = await message.Content.ReadAsStringAsync();
            throw new CoomerHTTPException(message.StatusCode, url, errorContent);
        }

        var json = await message.Content.ReadAsStringAsync();

        try
        {
            var posts = JsonSerializer.Deserialize<List<CoomerPost>>(json, serializerOptions);

            if (posts is null || posts.Count == 0)
            {
                throw new CoomerPostNotFoundException(0);
            }

            return posts[0];
        }
        catch (JsonException ex)
        {
            throw new CoomerDeserializationException("Failed to deserialize latest post response", url, json, ex);
        }
    }
}