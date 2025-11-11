using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace House.House.Services.Gooning.Exceptions;

public class CoomerHTTPException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Url { get; }
    public string? ResponseContent { get; }

    public CoomerHTTPException(HttpStatusCode statusCode, string? url = null, string? content = null, string? message = null) : base(message ?? $"HTTP {(int)statusCode} returned from Coomer.su")
    {
        StatusCode = statusCode;
        Url = url;
        ResponseContent = content;
    }

    public override string ToString()
    {
        return $"CoomerHTTPException: {Message} (Status: {(int)StatusCode})" +
                (Url != null ? $", URL: {Url}" : "") +
                (ResponseContent != null ? $"\nResponse: {ResponseContent}" : "");
    }
}

public class CoomerCreatorNotFoundException : Exception
{
    public string Service { get; }
    public string Username { get; }

    public CoomerCreatorNotFoundException(string service, string username)
        : base($"Creator '{service}/{username}' was not found.")
    {
        Service = service;
        Username = username;
    }

    public override string ToString()
    {
        return $"CoomerCreatorNotFoundException: {Message} (Service: {Service}, Username: {Username})";
    }
}

public class CoomerPostNotFoundException : Exception
{
    public long PostId { get; }

    public CoomerPostNotFoundException(long postId)
        : base($"Post with ID '{postId}' was not found.")
    {
        PostId = postId;
    }

    public override string ToString()
    {
        return $"CoomerPostNotFoundException: {Message} (Post ID: {PostId})";
    }
}

public class CoomerDeserializationException : Exception
{
    public string? Url { get; }
    public string? RawContent { get; }

    public CoomerDeserializationException(string message, string? url = null, string? rawContent = null, Exception? inner = null)
        : base(message, inner)
    {
        Url = url;
        RawContent = rawContent;
    }

    public override string ToString()
    {
        return $"CoomerDeserializationException: {Message}" +
               (Url != null ? $"\nURL: {Url}" : "") +
               (RawContent != null ? $"\nContent: {RawContent}" : "") +
               (InnerException != null ? $"\nInner: {InnerException.Message}" : "");
    }
}

public class CoomerClientException : Exception
{
    public CoomerClientException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }

    public override string ToString() => $"CoomerClientException: {Message}" +
                                         (InnerException != null ? $"\nInner: {InnerException.Message}" : "");
}

public class CoomerPostsNotFoundException : Exception
{
    public string Service { get; }
    public string Username { get; }

    public CoomerPostsNotFoundException(string service, string username)
        : base($"Posts not found for service '{service}' and username '{username}'.")
    {
        Service = service;
        Username = username;
    }

    public override string ToString()
    {
        return $"CoomerPostsNotFoundException: {Message} (Service: {Service}, Username: {Username})";
    }
}