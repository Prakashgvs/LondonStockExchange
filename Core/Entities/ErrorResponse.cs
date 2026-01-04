using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;


namespace Core.Entities
{

    public class ErrorResponse
    {
        public DateTime Timestamp { get; set; }
        public int Status { get; set; }
        public string Error { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string CorrelationId { get; set; } = default!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string>? Details { get; set; }
    }

}
