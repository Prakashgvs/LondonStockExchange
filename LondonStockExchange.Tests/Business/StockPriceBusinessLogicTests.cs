using Business.BusinessLogic;
using LondonStockExchange.Tests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Core.Interfaces;
using Xunit;
using FluentAssertions;

namespace LondonStockExchange.Tests.Business
{
    public class StockPriceBusinessLogicTests
    {
        private readonly Mock<IStockPriceRepository> _repo = new();

        [Fact]
        public async Task GetStockPrice_ShouldCalculateAverageCorrectly()
        {
            _repo.Setup(x => x.GetByTickerAsync("BEL"))
                .ReturnsAsync(TestDataFactory.StockSummary(1000, 10));

            var logic = new StockPriceBusinessLogic(_repo.Object);

            var result = await logic.GetStockPriceAsync("BEL");

            result!.AveragePrice.Should().Be(100);
            result.TotalShares.Should().Be(10);
        }
    }
}
