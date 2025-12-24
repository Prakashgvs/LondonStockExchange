using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IStockPriceRepository
    {
        Task<StockSummary?> GetByTickerAsync(string tickerSymbol);
        Task<List<StockSummary>> GetByTickersAsync(List<string> tickers);
        Task<List<StockSummary>> GetAllAsync();
    }
}
