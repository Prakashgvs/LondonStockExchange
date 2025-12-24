using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class StockSummary
    {
        public string TickerSymbol { get; set; } = default!;
        public decimal TotalValue { get; set; }
        public decimal TotalShares { get; set; }
        public long TransactionCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
