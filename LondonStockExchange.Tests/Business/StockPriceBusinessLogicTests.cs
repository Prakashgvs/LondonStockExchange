using Business.BusinessLogic;
using Core.Entities;
using Core.Exceptions;
using Core.Interfaces;
using FluentAssertions;
using LondonStockExchange.Tests.TestHelpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LondonStockExchange.Tests.Business
{
    public class TradeBusinessLogicTests
    {
        private readonly Mock<ITradeRepository> _tradeRepo = new();
        private readonly Mock<ITradeQueue> _tradeQueue = new();
        private readonly Mock<IApplicationLogger> _logger = new();

        [Fact]
        public async Task RecordTrade_WithValidRequest_ShouldEnqueueSuccessfully()
        {
            var request = TestDataFactory.ValidTradeRequest();
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var result = await logic.RecordTradeAsync(request);

            result.Should().NotBeNull();
            result.TickerSymbol.Should().Be("BEL");
            result.Price.Should().Be(100);
            result.Shares.Should().Be(10);
            result.TradeValue.Should().Be(1000);
            result.Message.Should().Be("Trade accepted for processing");
            _tradeQueue.Verify(x => x.EnqueueAsync(It.IsAny<TradeMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RecordTrade_WithNullRequest_ShouldThrowValidationException()
        {
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var act = async () => await logic.RecordTradeAsync(null!);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Trade request cannot be null");
        }

        [Fact]
        public async Task RecordTrade_WithZeroPrice_ShouldThrowValidationException()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Price = 0;
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var act = async () => await logic.RecordTradeAsync(request);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Price and quantity must be greater than zero");
        }

        [Fact]
        public async Task RecordTrade_WithNegativePrice_ShouldThrowValidationException()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Price = -100;
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var act = async () => await logic.RecordTradeAsync(request);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Price and quantity must be greater than zero");
        }

        [Fact]
        public async Task RecordTrade_WithZeroQuantity_ShouldThrowValidationException()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Quantity = 0;
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var act = async () => await logic.RecordTradeAsync(request);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Price and quantity must be greater than zero");
        }

        [Fact]
        public async Task RecordTrade_WithNegativeQuantity_ShouldThrowValidationException()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Quantity = -10;
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var act = async () => await logic.RecordTradeAsync(request);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("Price and quantity must be greater than zero");
        }

        [Fact]
        public async Task RecordTrade_ShouldCalculateTradeValueCorrectly()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Price = 123.45m;
            request.Quantity = 50;
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var result = await logic.RecordTradeAsync(request);

            result.TradeValue.Should().Be(6172.50m);
        }

        [Fact]
        public async Task RecordTrade_ShouldNormalizeTickerToUpperCase()
        {
            var request = TestDataFactory.ValidTradeRequest();
            request.Ticker = "aapl";
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var result = await logic.RecordTradeAsync(request);

            result.TickerSymbol.Should().Be("AAPL");
        }

        [Fact]
        public async Task RecordTrade_ShouldGenerateUniqueTradeId()
        {
            var request = TestDataFactory.ValidTradeRequest();
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var result1 = await logic.RecordTradeAsync(request);
            var result2 = await logic.RecordTradeAsync(request);

            result1.TradeId.Should().NotBe(result2.TradeId);
            result1.TradeId.Should().NotBeEmpty();
            result2.TradeId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task RecordTrade_ShouldSetTimestampToUtcNow()
        {
            var request = TestDataFactory.ValidTradeRequest();
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);
            var beforeTime = DateTime.UtcNow;

            var result = await logic.RecordTradeAsync(request);

            var afterTime = DateTime.UtcNow;
            result.Timestamp.Should().BeOnOrAfter(beforeTime);
            result.Timestamp.Should().BeOnOrBefore(afterTime);
        }

        [Fact]
        public async Task RecordTrade_WhenQueueFails_ShouldPropagateException()
        {
            var request = TestDataFactory.ValidTradeRequest();
            _tradeQueue.Setup(x => x.EnqueueAsync(It.IsAny<TradeMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Queue is full"));
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            var act = async () => await logic.RecordTradeAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Queue is full");
        }

        [Fact]
        public async Task RecordTrade_ShouldPassCorrectDataToQueue()
        {
            var request = TestDataFactory.ValidTradeRequest();
            TradeMessage? capturedMessage = null;
            _tradeQueue.Setup(x => x.EnqueueAsync(It.IsAny<TradeMessage>(), It.IsAny<CancellationToken>()))
                .Callback<TradeMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
                .Returns(Task.CompletedTask);
            var logic = new TradeBusinessLogic(_tradeRepo.Object, _tradeQueue.Object, _logger.Object);

            await logic.RecordTradeAsync(request);

            capturedMessage.Should().NotBeNull();
            capturedMessage!.TickerSymbol.Should().Be("BEL");
            capturedMessage.Price.Should().Be(100);
            capturedMessage.Shares.Should().Be(10);
            capturedMessage.BrokerId.Should().Be("BRK1");
            capturedMessage.TradeValue.Should().Be(1000);
        }
    }
}
