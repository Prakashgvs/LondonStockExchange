using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Interfaces
{
    public interface IStockPriceBusinessLogic
    {
        Task<StockPrice?> GetStockPriceAsync(string ticker);
        Task<List<StockPrice>> GetStockPricesBatchAsync(StockBatchRequest request);
    }
}
