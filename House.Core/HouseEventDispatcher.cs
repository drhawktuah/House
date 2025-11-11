using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace House.House.Core;

public sealed class HouseEventDispatcher
{
    public IReadOnlyDictionary<string, HouseBaseEvent> Events => events;
    private readonly Dictionary<string, HouseBaseEvent> events = [];

    private readonly ILogger logger;

    public HouseEventDispatcher(ILogger logger)
    {
        this.logger = logger;
    }

    public async Task RegisterAll(DiscordClient client)
    {
        await RegisterDiscordEvents(client);

        var commandsNext = client.GetCommandsNext();
        if (commandsNext != null)
        {
            await RegisterCommandsNextEvents(commandsNext);
        }
    }

    public async Task RegisterHouseEventAsync<TSender, TEvent>(TSender sender, TEvent houseEvent) where TEvent : HouseBaseEvent
    {
        ArgumentNullException.ThrowIfNull(houseEvent);
        ArgumentNullException.ThrowIfNull(sender);

        var senderType = sender.GetType();
        var eventInfo = senderType.GetEvent(houseEvent.Name, BindingFlags.Public | BindingFlags.Instance);

        if (eventInfo is null)
        {
            throw new InvalidOperationException($"Event '{houseEvent.Name}' not found on '{senderType.Name}'");
        }

        var handlerType = eventInfo.EventHandlerType;
        if (handlerType is null)
        {
            throw new InvalidOperationException($"Event '{houseEvent.Name}' has no delegate type");
        }

        var mainAsync = houseEvent.GetType().GetMethod(nameof(HouseBaseEvent.MainAsync));
        if (mainAsync is null)
        {
            throw new InvalidOperationException($"{houseEvent.GetType().Name} must implement MainAsync()");
        }

        try
        {
            var delegateInstance = Delegate.CreateDelegate(handlerType, houseEvent, mainAsync);
            eventInfo.AddEventHandler(sender, delegateInstance);

            events[eventInfo.Name] = houseEvent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"error initializing {houseEvent.Name}: {ex}");
        }

        await Task.CompletedTask;
    }

    private async Task RegisterCommandsNextEvents(CommandsNextExtension commandsNext)
    {
        ArgumentNullException.ThrowIfNull(commandsNext);

        var baseEventType = typeof(HouseCommandsNextEvent);
        var allEventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract && baseEventType.IsAssignableFrom(t))
            .ToArray();

        var eventsFound = typeof(CommandsNextExtension).GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (var evt in eventsFound)
        {
            var handlerType = evt.EventHandlerType;
            var invokeMethod = handlerType?.GetMethod("Invoke");
            if (invokeMethod is null)
            {
                continue;
            }

            var parameters = invokeMethod.GetParameters();
            if (parameters.Length != 2)
            {
                continue;
            }

            var senderType = parameters[0].ParameterType;
            var argsType = parameters[1].ParameterType;

            if (!typeof(CommandsNextExtension).IsAssignableFrom(senderType))
            {
                continue;
            }

            if (!typeof(CommandEventArgs).IsAssignableFrom(argsType))
            {
                continue;
            }

            var matchingType = allEventTypes.FirstOrDefault(t => string.Equals(t.Name, evt.Name + "Event", StringComparison.OrdinalIgnoreCase));

            HouseCommandsNextEvent houseEvent;
            if (matchingType != null)
            {
                houseEvent = (HouseCommandsNextEvent)Activator.CreateInstance(matchingType)!;
                logger.LogInformation("Initialized event '{HouseEventName}'", matchingType.Name);
            }
            else
            {
                houseEvent = new HouseCommandsNextEvent(evt.Name);
                logger.LogInformation("Initialized default CommandsNext event '{EventName}'", evt.Name);
            }

            await RegisterHouseEventAsync(commandsNext, houseEvent);
        }
    }

    private async Task RegisterDiscordEvents(DiscordClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var baseEventType = typeof(HouseBotEvent);
        var allEventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => !t.IsAbstract && baseEventType.IsAssignableFrom(t))
            .ToArray();

        foreach (var evt in typeof(DiscordClient).GetEvents(BindingFlags.Public | BindingFlags.Instance))
        {
            var handlerType = evt.EventHandlerType;
            var invokeMethod = handlerType?.GetMethod("Invoke");
            if (invokeMethod is null)
            {
                continue;
            }

            var parameters = invokeMethod.GetParameters();
            if (parameters.Length != 2)
            {
                continue;
            }

            var senderType = parameters[0].ParameterType;
            var argsType = parameters[1].ParameterType;

            if (!typeof(DiscordClient).IsAssignableFrom(senderType))
            {
                continue;
            }

            if (!typeof(DiscordEventArgs).IsAssignableFrom(argsType))
            {
                continue;
            }

            var matchingType = allEventTypes.FirstOrDefault(t => string.Equals(t.Name, evt.Name + "Event", StringComparison.OrdinalIgnoreCase));

            HouseBotEvent houseEvent;
            if (matchingType != null)
            {
                houseEvent = (HouseBotEvent) Activator.CreateInstance(matchingType)!;
                logger.LogInformation("Initialized event '{HouseEventName}'", matchingType.Name);
            }
            else
            {
                houseEvent = new HouseBotEvent(evt.Name);
            }

            await RegisterHouseEventAsync(client, houseEvent);
        }
    }
}
