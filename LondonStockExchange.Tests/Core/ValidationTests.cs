using Core.Entities;
using FluentAssertions;
using LondonStockExchange.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LondonStockExchange.Tests.Core
{
    public class TradeRequestValidationTests
    {
        [Fact]
        public void TradeRequest_WithValidData_ShouldPass()
        {
            var request = TestDataFactory.ValidTradeRequest();

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }

        [Fact]
        public void TradeRequest_WithInvalidTicker_ShouldFail()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Ticker = "###";

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void TradeRequest_WithEmptyTicker_ShouldFail()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Ticker = "";

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void TradeRequest_WithNullTicker_ShouldFail()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Ticker = null!;

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void TradeRequest_WithEmptyBrokerId_ShouldFail()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.BrokerId = "";

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
        }

        [Fact]
        public void TradeRequest_WithNullBrokerId_ShouldFail()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.BrokerId = null!;

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
        }
    }
}
