using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class TradeResponse
    {
        public Guid TradeId { get; set; }
        public string TickerSymbol { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal Shares { get; set; }
        public string BrokerId { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public decimal TradeValue { get; set; }
        public string Message { get; set; } = default!;
    }

}
