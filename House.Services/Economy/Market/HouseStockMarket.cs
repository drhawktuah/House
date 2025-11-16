using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace House.House.Services.Economy.Market;

public class HouseStockMarket
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string ID { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("stock_market_name")]
    public required string Name { get; set; }

    [BsonElement("symbol")]
    public required string Symbol { get; set; }

    [BsonElement("current_price")]
    public decimal CurrentPrice { get; set; }

    [BsonElement("previous_close_price")]
    public decimal PreviousClosePrice { get; set; }

    [BsonElement("volatility")]
    public double Volatility { get; set; } = 0.05;

    [BsonElement("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;

    [BsonElement("price_history")]
    public List<decimal> PriceHistory { get; set; } = [];

    [BsonElement("description")]
    public string Description { get; set; } = "No description available.";
}
