using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Gooning.Exceptions;

public class CoomerServiceException : Exception
{
    public string? Service { get; }

    public CoomerServiceException(string message, string? service = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Service = service;
    }

    public override string ToString()
    {
        return $"CoomerServiceException: {Message}" +
                (Service != null ? $" (Service: {Service})" : "") +
                (InnerException != null ? $"\nInner: {InnerException.Message}" : "");
    }
}