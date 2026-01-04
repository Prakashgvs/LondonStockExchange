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

        public static StockSummary StockSummary(decimal totalValue, int totalShares, string ticker = "BEL")
        {
            return new StockSummary
            {
                TickerSymbol = ticker,
                TotalValue = totalValue,
                TotalShares = totalShares,
                TransactionCount = 5,
                LastUpdated = DateTime.UtcNow
            };
        }

        public static TradeMessage TradeMessage(string ticker = "BEL", decimal price = 100, int shares = 10, string brokerId = "BRK1")
        {
            return new TradeMessage
            {
                TradeId = Guid.NewGuid(),
                TickerSymbol = ticker,
                Price = price,
                Shares = shares,
                BrokerId = brokerId,
                TradeValue = price * shares,
                OccurredAtUtc = DateTime.UtcNow
            };
        }

        public static StockBatchRequest StockBatchRequest(params string[] tickers)
        {
            return new StockBatchRequest
            {
                Tickers = tickers.ToList()
            };
        }
    }

}
