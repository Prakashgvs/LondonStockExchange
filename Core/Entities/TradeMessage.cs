using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Serializable]
    public class TradeMessage
    {
        public Guid TradeId { get; set; }

        public string TickerSymbol { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal Shares { get; set; }

        public string BrokerId { get; set; } = string.Empty;

        public decimal TradeValue { get; set; }

        public DateTime OccurredAtUtc { get; set; }
    }

}
