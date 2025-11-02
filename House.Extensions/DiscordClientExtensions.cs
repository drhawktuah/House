using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace House.House.Extensions;

[Obsolete("Marked as obsolete in favor of the new event registering system")]
public static partial class DiscordClientExtensions
{
    public static Dictionary<string, (object Target, EventInfo EventInfo, Delegate DelegateInstance)> Events => [];

    [Obsolete("Marked as obsolete in favor of the new event registering system")]
    public static async Task RegisterEventsAsync(this DiscordClient client)
    {
        var commandsNext = client.GetCommandsNext();

        foreach (var discordEvent in GetBotEvents<DiscordClientEventAttribute>())
        {
            await RegisterEventAsync<DiscordClientEventAttribute>(client, discordEvent);

            client.Logger.LogInformation("'{EventName}' loaded successfully", discordEvent.Name);
        }

        foreach (var commandsEvent in GetBotEvents<CommandsNextEventAttribute>())
        {
            await RegisterEventAsync<CommandsNextEventAttribute>(commandsNext, commandsEvent);

            client.Logger.LogInformation("'{EventName}' loaded successfully", commandsEvent.Name);
        }

        await Task.CompletedTask;
    }

    [Obsolete("Marked as obsolete in favor of the new event registering system")]
    public static async Task UnregisterEventsAsync(this DiscordClient client)
    {
        var commandsNext = client.GetCommandsNext();

        foreach (var discordEvent in GetBotEvents<DiscordClientEventAttribute>())
        {
            await UnregisterEventAsync<DiscordClientEventAttribute>(client, discordEvent);
            client.Logger.LogInformation("'{EventName}' unloaded successfully", discordEvent.Name);
        }

        foreach (var commandsEvent in GetBotEvents<CommandsNextEventAttribute>())
        {
            await UnregisterEventAsync<CommandsNextEventAttribute>(commandsNext, commandsEvent);
            client.Logger.LogInformation("'{EventName}' unloaded successfully", commandsEvent.Name);
        }

        await Task.CompletedTask;
    }

    [Obsolete("Marked as obsolete in favor of the new event registering system")]
    public static async Task RegisterEventAsync<TAttribute>(object target, MethodInfo method) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(method);

        var type = target.GetType();
        var attribute = method.GetCustomAttribute<TAttribute>();

        string eventName = attribute switch
        {
            DiscordClientEventAttribute discordClient => discordClient.Name,
            CommandsNextEventAttribute commandsNext => commandsNext.Name,
            _ => method.Name,
        };

        var eventInfo = type.GetEvent(eventName);
        if (eventInfo is null)
        {
            Console.WriteLine($"{method.Name} is not a valid event on {nameof(target)}");
            return;
        }

        if (eventInfo.EventHandlerType == null)
        {
            Console.WriteLine($"{eventInfo.Name} EventHandlerType is null");
            return;
        }

        try
        {
            var delegateInstance = method.CreateDelegate(eventInfo.EventHandlerType);
            eventInfo.AddEventHandler(target, delegateInstance);

            Events[eventInfo.Name] = (target, eventInfo, delegateInstance);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error initializing {eventInfo.Name}: {ex}");
        }

        await Task.CompletedTask;
    }

    [Obsolete("Marked as obsolete in favor of the new event registering system")]
    public static async Task UnregisterEventAsync<TAttribute>(object target, MethodInfo method) where TAttribute : Attribute
    {
        ArgumentNullException.ThrowIfNull(method);

        var type = target.GetType();
        var attribute = method.GetCustomAttribute<TAttribute>();

        string eventName = attribute switch
        {
            DiscordClientEventAttribute discordClient => discordClient.Name,
            CommandsNextEventAttribute commandsNext => commandsNext.Name,
            _ => method.Name,
        };

        var eventInfo = type.GetEvent(eventName);
        if (eventInfo is null || eventInfo.EventHandlerType == null)
        {
            return;
        }

        var obtainedEvent = Events.FirstOrDefault(e => e.Value.Target == target && e.Key == eventInfo.Name);

        if (obtainedEvent.Value.DelegateInstance != null)
        {
            try
            {
                eventInfo.RemoveEventHandler(target, obtainedEvent.Value.DelegateInstance);

                Events.Remove(eventInfo.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error whilst unregistering {eventInfo.Name}: {ex}");
            }
        }

        await Task.CompletedTask;
    }

    private static IEnumerable<MethodInfo> GetBotEvents<T>() where T : Attribute
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            .Where(m => m.GetCustomAttribute<T>() is not null);
    }
}

[Obsolete("Marked as obsolete in favor of the new event registering system")]
[AttributeUsage(AttributeTargets.Method)]
public class CommandsNextEventAttribute : Attribute
{
    public string Name { get; }

    public CommandsNextEventAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }
}

[Obsolete("Marked as obsolete in favor of the new event registering system")]
[AttributeUsage(AttributeTargets.Method)]
public class DiscordClientEventAttribute : Attribute
{
    public string Name { get; }

    public DiscordClientEventAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }
}