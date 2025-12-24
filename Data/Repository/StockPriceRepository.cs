using Core.Entities;
using Core.Interfaces;
using Data.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Data.Repository
{
    public class StockPriceRepository : IStockPriceRepository
    {
        private readonly IDatabase _database;

        public StockPriceRepository(IDatabase db)
        {
            _database = db;
        }

        public async Task<StockSummary?> GetByTickerAsync(string tickerSymbol)
        {
            try
            {
                return await _database.QuerySingleAsync<StockSummary>(
                    "sp_GetStockSummaryByTicker",
                    new { TickerSymbol = tickerSymbol }
                );
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public async Task<List<StockSummary>> GetByTickersAsync(List<string> tickers)
        {
            var table = new DataTable();
            table.Columns.Add("TickerSymbol", typeof(string));

            foreach (var ticker in tickers)
                table.Rows.Add(ticker);

            return (await _database.QueryAsync<StockSummary>(
                "sp_GetStockSummariesByTickers",
                new { Tickers = table }
            )).ToList();
        }

        public async Task<List<StockSummary>> GetAllAsync()
        {
            return (await _database.QueryAsync<StockSummary>(
                "sp_GetAllStockSummaries"
            )).ToList();
        }
    }
}
