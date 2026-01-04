using Core.Entities;
using Core.Exceptions;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.BusinessLogic
{
    public class TradeBusinessLogic : ITradeBusinessLogic
    {
        private readonly ITradeRepository _tradeRepository;
        private readonly ITradeQueue _tradeQueue;
        private readonly IApplicationLogger _logger;

        public TradeBusinessLogic(ITradeRepository tradeRepository, ITradeQueue tradeQueue, IApplicationLogger logger)
        {
            _tradeRepository = tradeRepository;
            _tradeQueue = tradeQueue;
            _logger = logger;
        }

        public async Task<TradeResponse> RecordTradeAsync(TradeRequest request)
        {
            if (request == null)
            {
                _logger.LogWarning("Trade request received as null");
                throw new ValidationException("Trade request cannot be null");
            }

            if (request.Price <= 0 || request.Quantity <= 0)
            {
                _logger.LogWarning("Invalid trade values - BrokerId: {BrokerId}, Ticker: {Ticker}, Price: {Price}, Quantity: {Quantity}",
                    request.BrokerId, request.Ticker, request.Price, request.Quantity);
                throw new ValidationException("Price and quantity must be greater than zero");
            }

            _logger.LogInformation("Processing trade - BrokerId: {BrokerId}, Ticker: {Ticker}, Price: {Price}, Quantity: {Quantity}",
                request.BrokerId, request.Ticker, request.Price, request.Quantity);

            var tradeValue = request.Price * request.Quantity;

            var tradeMessage = new TradeMessage
            {
                TradeId = Guid.NewGuid(),
                TickerSymbol = request.Ticker.ToUpperInvariant(),
                Price = request.Price,
                Shares = request.Quantity,
                BrokerId = request.BrokerId,
                TradeValue = tradeValue,
                OccurredAtUtc = DateTime.UtcNow
            };

            _logger.LogDebug("Trade message created - TradeId: {TradeId}, TickerSymbol: {TickerSymbol}, TradeValue: {TradeValue}",
                tradeMessage.TradeId, tradeMessage.TickerSymbol, tradeMessage.TradeValue);

            await _tradeQueue.EnqueueAsync(tradeMessage, CancellationToken.None);

            _logger.LogInformation("Trade enqueued successfully - TradeId: {TradeId}, BrokerId: {BrokerId}, Ticker: {Ticker}",
                tradeMessage.TradeId, request.BrokerId, tradeMessage.TickerSymbol);

            return new TradeResponse
            {
                TradeId = tradeMessage.TradeId,
                TickerSymbol = tradeMessage.TickerSymbol,
                Price = tradeMessage.Price,
                Shares = tradeMessage.Shares,
                BrokerId = tradeMessage.BrokerId,
                Timestamp = tradeMessage.OccurredAtUtc,
                TradeValue = tradeValue,
                Message = "Trade accepted for processing"
            };
        }
    }
}
