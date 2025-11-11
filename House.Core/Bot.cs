using System.Reflection;
using System.Security.Cryptography;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using House.House.Modules;
using House.House.Extensions;
using House.House.Services.Database;
using House.House.Services.Economy;
using House.House.Services.Fuzzy;
using House.House.Services.Gooning.HTTP;
using House.House.Services.Protection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace House.House.Core;

public sealed class Bot
{
    public DiscordClient Client => client;

    private readonly DiscordClient client;
    private readonly Config config;
    private readonly CommandsNextExtension commandsNext;
    private readonly HouseEventDispatcher dispatcher;

    public Bot()
    {
        config = Config.Deserialize("config.json");

        client = new DiscordClient(BuildDiscordConfig());
        client.UseInteractivity(BuildInteractivityConfig());

        var services = ConfigureServices();

        commandsNext = services.GetRequiredService<CommandsNextExtension>();
        commandsNext.RegisterCommands(Assembly.GetExecutingAssembly());

        dispatcher = new(client.Logger);
    }

    public async Task StartAsync()
    {
        await dispatcher.RegisterAll(client);

        await client.InitializeAsync();
        await client.ConnectAsync();

        commandsNext.Services.GetRequiredService<AntiNukeService>(); // please ignore this, it just forces the anti-nuke service to initialize

        await Task.Delay(-1);
    }

    private DiscordConfiguration BuildDiscordConfig() => new()
    {
        Token = config.Token,
        TokenType = TokenType.Bot,
        Intents = DiscordIntents.All,
        AutoReconnect = true
    };

    private static CommandsNextConfiguration BuildCommandsNextConfig(IServiceProvider services) => new()
    {
        CaseSensitive = false,
        UseDefaultCommandHandler = false,
        EnableDefaultHelp = false,
        Services = services
    };

    private static InteractivityConfiguration BuildInteractivityConfig() => new()
    {
        Timeout = TimeSpan.FromSeconds(30),
        PollBehaviour = DSharpPlus.Interactivity.Enums.PollBehaviour.DeleteEmojis
    };

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(config);
        services.AddSingleton(client);
        services.AddSingleton(client.GetInteractivity());

        services.AddSingleton(sp =>
        {
            var commands = client.UseCommandsNext(BuildCommandsNextConfig(sp));
            return commands;
        });

        services.AddSingleton<HouseFuzzyMatchingService>();
        services.AddSingleton<IMongoClient>(sp => new MongoClient(config.DBConnectionURL));

        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("houseirony");
        });

        services.AddSingleton(sp => new HouseEconomyDatabase(sp.GetRequiredService<IMongoClient>()));
        services.AddSingleton(sp => new GuildRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new WhitelistedUserRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new BlacklistedUserRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new StaffUserRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new StarboardRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new BackupRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new BackupService(sp.GetRequiredService<BackupRepository>()));
        services.AddSingleton(sp => new SnipeRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new SuspectMemberRepository(sp.GetRequiredService<IMongoDatabase>()));
        services.AddSingleton(sp => new AntiNukeService(client));

        services.AddSingleton(sp =>
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.ParseAdd("text/css");

            return client;
        });

        services.AddSingleton<CoomerCache>();
        services.AddSingleton<UserDataCache>();

        services.AddSingleton(sp =>
        {
            return new CoomerClient(
                sp.GetRequiredService<HttpClient>(),
                sp.GetRequiredService<UserDataCache>(),
                sp.GetRequiredService<CoomerCache>());
        });

        services.AddSingleton(sp =>
        {
            var db = sp.GetRequiredService<IMongoDatabase>();
            return new EncryptedStorageService<UserCoomerData>(db, "coomer_collection");
        });

        return services.BuildServiceProvider();
    }
}