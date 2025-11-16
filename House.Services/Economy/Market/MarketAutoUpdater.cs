using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace House.House.Services.Economy.Market;

public sealed class MarketAutoUpdater : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly TimeSpan interval;
    private CancellationTokenSource? _cts;
    private Task? _backgroundTask;

    public MarketAutoUpdater(TimeSpan interval)
    {
        logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<MarketAutoUpdater>();
        this.interval = interval;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = Task.Run(() => RunAsync(_cts.Token), cancellationToken);

        logger.LogInformation("MarketAutoUpdater service started");

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("MarketAutoUpdater service stopping");

        if (_cts != null)
        {
            _cts.Cancel();
            try
            {
                if (_backgroundTask != null)
                {
                    await _backgroundTask.ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing, expected on shutdown
            }
        }

        logger.LogInformation("MarketAutoUpdater stopped");
    }

    private async Task RunAsync(CancellationToken token)
    {
        logger.LogInformation("MarketAutoUpdater background loop started.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await UpdateAllMarketsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while updating markets.");
            }

            try
            {
                await Task.Delay(interval, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("MarketAutoUpdater background loop stopped.");
    }

    private Task UpdateAllMarketsAsync()
    {
        foreach (HouseStockMarket market in MarketPresets.GetActiveStocks())
        {
            decimal changePercent = (decimal)(NextDouble() * 2 - 1) * (decimal)market.Volatility;
            decimal newPrice = market.CurrentPrice * (1 + changePercent);

            market.PreviousClosePrice = market.CurrentPrice;
            market.CurrentPrice = Math.Max(newPrice, 0.01m);
            market.LastUpdated = DateTime.UtcNow;

            market.PriceHistory.Add(market.CurrentPrice);
            if (market.PriceHistory.Count > 50)
            {
                market.PriceHistory.RemoveAt(0);
            }

            logger.LogInformation("Updated {MarketName} ({Symbol}) price to {Price:C}", market.Name, market.Symbol, market.CurrentPrice);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private static int NextInt(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max);
    }

    private static double NextDouble()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return (BitConverter.ToUInt64(bytes) >> 11) / (double)(1UL << 53);
    }
}