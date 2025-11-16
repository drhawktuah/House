using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Economy.Market;

public static class MarketPresets
{
    public static readonly List<HouseStockMarket> StockPool = [
        new HouseStockMarket
        {
            Name = "Iron Ore Exchange",
            Symbol = "IOX",
            CurrentPrice = 100m,
            PreviousClosePrice = 95m,
            Volatility = 0.05,
            Description = "A market for trading iron ore."
        },
        new HouseStockMarket
        {
            Name = "Gold Exchange",
            Symbol = "GLD",
            CurrentPrice = 500m,
            PreviousClosePrice = 480m,
            Volatility = 0.07,
            Description = "High-value gold stocks."
        },
        new HouseStockMarket
        {
            Name = "Platinum Exchange",
            Symbol = "PLT",
            CurrentPrice = 1000m,
            PreviousClosePrice = 950m,
            Volatility = 0.1,
            Description = "Premium platinum stocks."
        }
    ];

    public static IEnumerable<HouseStockMarket> GetActiveStocks()
    {
        foreach (var stock in StockPool)
        {
            if (stock.IsActive)
            {
                yield return stock;
            }
        }
    }
    
    public static HouseStockMarket? FindBySymbol(string symbol)
    {
        return StockPool.Find(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
    }
}
