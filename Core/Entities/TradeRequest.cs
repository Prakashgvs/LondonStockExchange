using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Core.Entities
{
    public class TradeRequest
    {
        [Required]
        [StringLength(10, MinimumLength = 1)]
        [RegularExpression("^[A-Z0-9.]+$")]
        public string Ticker { get; set; } = default!;

        [Required]
        [Range(typeof(decimal), "0.0001", "999999999999999")]
        public decimal Price { get; set; }

        [Required]
        [Range(typeof(decimal), "0.000001", "9999999999999")]
        public decimal Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string BrokerId { get; set; } = default!;
    }
}
