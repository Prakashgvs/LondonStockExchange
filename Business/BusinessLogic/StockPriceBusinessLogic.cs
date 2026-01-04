using Core.Entities;
using Core.Exceptions;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.BusinessLogic
{
    public class StockPriceBusinessLogic : IStockPriceBusinessLogic
    {
        private readonly IStockPriceRepository _stockPriceRepository;
        private readonly IApplicationLogger _logger;

        public StockPriceBusinessLogic(IStockPriceRepository stockPriceRepository, IApplicationLogger logger)
        {
            _stockPriceRepository = stockPriceRepository;
            _logger = logger;
        }

        public async Task<StockPrice?> GetStockPriceAsync(string ticker)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                _logger.LogWarning("Empty ticker symbol requested");
                throw new ValidationException("Ticker symbol cannot be empty");
            }

            if (ticker.Length > 10)
            {
                _logger.LogWarning("Ticker symbol too long - Ticker: {Ticker}, Length: {Length}", ticker, ticker.Length);
                throw new ValidationException("Ticker symbol cannot exceed 10 characters");
            }

            _logger.LogInformation("Fetching stock price for ticker: {Ticker}", ticker);

            var summary = await _stockPriceRepository.GetByTickerAsync(ticker.ToUpperInvariant());

            if (summary == null)
            {
                _logger.LogInformation("Stock price not found for ticker: {Ticker}", ticker);
                return null;
            }

            _logger.LogDebug("Stock price retrieved - Ticker: {Ticker}, TotalShares: {TotalShares}, TransactionCount: {TransactionCount}",
                summary.TickerSymbol, summary.TotalShares, summary.TransactionCount);

            return new StockPrice
            {
                TickerSymbol = summary.TickerSymbol,
                AveragePrice = summary.TotalShares == 0 ? 0 : summary.TotalValue / summary.TotalShares,
                TotalShares = summary.TotalShares,
                TransactionCount = summary.TransactionCount,
                LastUpdated = summary.LastUpdated
            };
        }

        public async Task<List<StockPrice>> GetStockPricesBatchAsync(StockBatchRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Batch request received as null");
                throw new ValidationException("Batch request cannot be null");
            }

            if (request.Tickers == null || request.Tickers.Count == 0)
            {
                _logger.LogInformation("Fetching all tickers - no specific tickers requested");
                return await GetAllTickersAsync();
            }

            if (request.Tickers.Count > 100) //configurable limit
            {
                _logger.LogWarning("Batch request exceeds limit - RequestedCount: {Count}", request.Tickers.Count);
                throw new ValidationException("Cannot request more than 100 tickers at once");
            }

            _logger.LogInformation("Processing batch request - TickerCount: {Count}", request.Tickers.Count);

            var tickers = request.Tickers
                .Select(t => t.ToUpperInvariant())
                .Distinct()
                .ToList();

            _logger.LogDebug("Distinct tickers after normalization - Count: {Count}", tickers.Count);

            var summaries = await _stockPriceRepository.GetByTickersAsync(tickers);

            _logger.LogInformation("Batch request completed - RequestedCount: {RequestedCount}, ReturnedCount: {ReturnedCount}",
                tickers.Count, summaries.Count);

            return summaries.Select(MapToStockPrice).ToList();
        }

        private async Task<List<StockPrice>> GetAllTickersAsync()
        {
            _logger.LogInformation("Fetching all available tickers");

            var summaries = await _stockPriceRepository.GetAllTickersAsync();

            _logger.LogInformation("All tickers fetched - Count: {Count}", summaries.Count);

            return summaries.Select(MapToStockPrice).ToList();
        }

        private static StockPrice MapToStockPrice(StockSummary summary)
        {
            return new StockPrice
            {
                TickerSymbol = summary.TickerSymbol,
                AveragePrice = summary.TotalShares == 0 ? 0 : summary.TotalValue / summary.TotalShares,
                TotalShares = summary.TotalShares,
                TransactionCount = summary.TransactionCount,
                LastUpdated = summary.LastUpdated
            };
        }
    }
}
