using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.BusinessLogic
{
    public class StockPriceBusinessLogic : IStockPriceBusinessLogic
    {
        private readonly IStockPriceRepository _stockPriceRepository;

        public StockPriceBusinessLogic(IStockPriceRepository stockPriceRepository)
        {
            _stockPriceRepository = stockPriceRepository;
        }

        public async Task<StockPrice?> GetStockPriceAsync(string ticker)
        {
            var summary = await _stockPriceRepository.GetByTickerAsync(ticker.ToUpperInvariant());

            if (summary == null)
                return null;

            return new StockPrice
            {
                TickerSymbol = summary.TickerSymbol,
                AveragePrice = summary.TotalShares == 0 ? 0: summary.TotalValue / summary.TotalShares,
                TotalShares = summary.TotalShares,
                TransactionCount = summary.TransactionCount,
                LastUpdated = summary.LastUpdated
            };
        }

        public async Task<List<StockPrice>> GetStockPricesAsync(StockBatchRequest request)
        {
            if (request.Tickers == null || request.Tickers.Count == 0)
            {
                return await GetAllAsync();
            }


            var tickers = request.Tickers
                .Select(t => t.ToUpperInvariant())
                .Distinct()
                .ToList();

            var summaries = await _stockPriceRepository.GetByTickersAsync(tickers);

            return summaries.Select(MapToStockPrice).ToList();
        }

        private async Task<List<StockPrice>> GetAllAsync()
        {
            var summaries = await _stockPriceRepository.GetAllAsync();
            return summaries.Select(MapToStockPrice).ToList();
        }

        private static StockPrice MapToStockPrice(StockSummary summary)
        {

            return new StockPrice
            {
                TickerSymbol = summary.TickerSymbol,
                AveragePrice = summary.TotalShares == 0 ? 0: summary.TotalValue / summary.TotalShares,
                TotalShares = summary.TotalShares,
                TransactionCount = summary.TransactionCount,
                LastUpdated = summary.LastUpdated
            };
        }
    }
}
