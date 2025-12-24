using Core.Entities;
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

        public TradeBusinessLogic(ITradeRepository tradeRepository, ITradeQueue tradeQueue)
        {
            _tradeRepository = tradeRepository;
            _tradeQueue = tradeQueue;
        }

        public async Task<TradeResponse> RecordTradeAsync(TradeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Price <= 0 || request.Quantity <= 0)
                throw new ArgumentException("Invalid trade values");


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

            await _tradeQueue.EnqueueAsync(tradeMessage, CancellationToken.None);

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
