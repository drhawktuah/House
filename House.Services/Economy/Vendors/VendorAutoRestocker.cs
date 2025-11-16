using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace House.House.Services.Economy.Vendors;

public sealed class VendorAutoRestocker : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly TimeSpan interval;
    private CancellationTokenSource? _cts;
    private Task? _backgroundTask;

    public VendorAutoRestocker(TimeSpan interval)
    {
        logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<VendorAutoRestocker>();
        this.interval = interval;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = Task.Run(() => RunAsync(_cts.Token));

        logger.LogInformation("VendorAutoRestocker service started");

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("VendorAutoRestocker service stopping");

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

        logger.LogInformation("VendorAutoRestocker stopped");
    }

    private async Task RunAsync(CancellationToken token)
    {
        logger.LogInformation("VendorAutoRestocker background loop started.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                await RestockAllVendorsAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while restocking vendors.");
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

        logger.LogInformation("VendorAutoRestocker background loop stopped.");
    }

    private Task RestockAllVendorsAsync()
    {
        foreach (var vendor in VendorPresets.VendorPool)
        {
            var elapsed = DateTime.UtcNow - vendor.LastRestockTime;
            if (elapsed >= vendor.RestockInterval)
            {
                int updatedCount = vendor.UpdateInventory();
                logger.LogInformation("Restocked {VendorName} with {Count} item updates.", vendor.Name, updatedCount);
                vendor.LastRestockTime = DateTime.UtcNow;
            }
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}