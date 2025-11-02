using House.House.Core;
using House.House.Services.Gooning.HTTP;
using MongoDB.Driver;

namespace House;

public class Program
{
    public static async Task Main(string[] args)
    {
        var bot = new Bot();
        await bot.StartAsync();

        //await Test();

        //GetTotalLines(".", "*.cs", true);
    }

    public static void GetTotalLines(string directoryPath, string searchPattern = "*.*", bool includeSubdirectories = false)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        int totalLines = 0;

        foreach (var filePath in Directory.GetFiles(directoryPath, searchPattern, searchOption))
        {
            try
            {
                int lineCount = File.ReadLines(filePath).Count();
                totalLines += lineCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not read file {filePath}: {ex.Message}");
            }
        }

        Console.WriteLine(totalLines);
    }

    public static async Task Test()
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };

        HttpClient client = new(handler);

        client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.ParseAdd("text/css");

        MongoClient mongoClient = new();

        IMongoDatabase database = mongoClient.GetDatabase("name");

        CoomerClient coomerClient = new(client, new UserDataCache(new EncryptedStorageService<UserCoomerData>(database, "lol")));

        foreach (var creator in await coomerClient.GetCreatorsAsync())
        {
            Console.WriteLine(creator.Name);
        }
    }
}