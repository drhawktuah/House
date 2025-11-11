using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace House.House.Services.Economy;

public class UserNotFoundException : Exception
{
    public ulong ID { get; }
    public DiscordUser? User { get; }

    public UserNotFoundException(ulong ID, DiscordUser? user = null) : base(user != null ? $"User '{user.Username}' is not a part of this economy." : $"User with ID {ID} is not a part of this economy.")
    {
        this.ID = ID;
        this.User = user;
    }
}

public class UserAlreadyExistsException : Exception
{
    public ulong ID { get; }
    public DiscordUser? User { get; }

    public UserAlreadyExistsException(ulong ID, DiscordUser? user = null)
        : base(user != null
            ? $"User '{user.Username}' is already part of this economy."
            : $"User with ID {ID} is already part of this economy.")
    {
        this.ID = ID;
        this.User = user;
    }
}

public class NoBalanceChangeProvidedException : Exception
{
    public NoBalanceChangeProvidedException()
        : base("No balance change was provided. You need to specify an amount to change.")
    {
    }
}