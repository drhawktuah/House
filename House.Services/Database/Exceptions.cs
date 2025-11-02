using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Database;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(object ID) : base($"Entity with ID '{ID}' was not found") { }
}

public class EntityExistsException : Exception
{
    public EntityExistsException(object ID) : base($"Entity with ID '{ID}' already exists") { }
}

public class RepositoryOperationException : Exception
{
    public RepositoryOperationException(string message, Exception? inner = null) : base(message, inner)
    {

    }
}

public class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception? inner) : base(message, inner) { }
}

public sealed class WhitelistedUserNotFoundException : RepositoryException
{
    public WhitelistedUserNotFoundException(ulong id) : base($"Whitelisted user with ID '{id}' was not found.") { }
}

public sealed class WhitelistedUserExistsException : RepositoryException
{
    public WhitelistedUserExistsException(ulong id)
        : base($"Whitelisted user with ID '{id}' already exists.") { }
}

public sealed class BlacklistedUserNotFoundException : RepositoryException
{
    public BlacklistedUserNotFoundException(ulong id)
        : base($"Blacklisted user with ID '{id}' was not found.") { }
}

public sealed class BlacklistedUserExistsException : RepositoryException
{
    public BlacklistedUserExistsException(ulong id)
        : base($"Blacklisted user with ID '{id}' already exists.") { }
}

public sealed class StaffUserNotFoundException : RepositoryException
{
    public StaffUserNotFoundException(ulong id)
        : base($"Staff user with ID '{id}' was not found.") { }
}

public sealed class StaffUserExistsException : RepositoryException
{
    public StaffUserExistsException(ulong id)
        : base($"Staff user with ID '{id}' already exists.") { }
}

public sealed class GuildNotFoundException : RepositoryException
{
    public GuildNotFoundException(ulong id)
        : base($"Guild with ID '{id}' was not found.") { }
}

public sealed class GuildExistsException : RepositoryException
{
    public GuildExistsException(ulong id)
        : base($"Guild with ID '{id}' already exists.") { }
}

public sealed class StarboardEntryNotFoundException : RepositoryException
{
    public StarboardEntryNotFoundException(ulong id)
        : base($"Starboard entry with ID '{id}' was not found.") { }
}

public sealed class StarboardEntryExistsException : RepositoryException
{
    public StarboardEntryExistsException(ulong id)
        : base($"Starboard entry with ID '{id}' already exists.") { }
}

public sealed class SuspectMemberNotFoundException : RepositoryException
{
    public SuspectMemberNotFoundException(ulong id)
        : base($"Suspect member with ID '{id}' was not found.") { }
}

public sealed class SuspectMemberExistsException : RepositoryException
{
    public SuspectMemberExistsException(ulong id)
        : base($"Suspect member with ID '{id}' already exists.") { }
}
