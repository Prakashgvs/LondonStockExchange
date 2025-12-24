using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class StockPrice
    {
        public string TickerSymbol { get; set; } = default!;
        public decimal AveragePrice { get; set; }
        public decimal TotalShares { get; set; }
        public long TransactionCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
