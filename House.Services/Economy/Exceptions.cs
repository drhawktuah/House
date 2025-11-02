using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Economy;

public class UserNotFoundException : Exception
{
    public ulong ID { get; }

    public UserNotFoundException(ulong ID) : base($"User with ID {ID} was not found in the database.")
    {
        this.ID = ID;
    }
}

public class UserAlreadyExistsException : Exception
{
    public ulong ID { get; }

    public UserAlreadyExistsException(ulong ID) : base($"User with ID {ID} already exists.")
    {
        this.ID = ID;
    }
}

public class NoBalanceChangeProvidedException : Exception
{
    public NoBalanceChangeProvidedException() : base("No balance change provided.")
    {
    }
}