using Core.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonStockExchange.Tests.Core
{
    public class ValidationTests
    {
        [Fact]
        public void TradeRequest_ShouldFail_WhenTickerIsInvalid()
        {
            var request = new TradeRequest
            {
                Ticker = "###",
                Price = 100,
                Quantity = 10,
                BrokerId = "BRK1"
            };

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                request, context, results, true);

            isValid.Should().BeFalse();
        }
    }
}
