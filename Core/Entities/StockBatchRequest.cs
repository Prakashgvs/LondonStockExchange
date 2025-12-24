using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public class StockBatchRequest
    {
        [JsonProperty("tickers")]
        public List<string> Tickers { get; set; } = new();
    }
}
