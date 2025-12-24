using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonStockExchange.Tests.TestHelpers
{
    public static class TestDataFactory
    {
        public static TradeRequest ValidTradeRequest() => new TradeRequest
        {
            Ticker = "BEL",
            Price = 100,
            Quantity = 10,
            BrokerId = "BRK1"
        };

        public static StockSummary StockSummary(decimal totalValue, decimal totalShares)
        {
            return new StockSummary
            {
                TickerSymbol = "BEL",
                TotalValue = totalValue,
                TotalShares = totalShares,
                TransactionCount = 5,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

}
