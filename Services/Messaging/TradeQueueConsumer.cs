using Core.Entities;
using Core.Interfaces;
using MSMQ.Messaging;
using Microsoft.Extensions.Configuration;

namespace Services.Messaging
{
    public class TradeQueueConsumer : BackgroundService
    {
        private readonly string _queuePath;
        private readonly ITradeRepository _tradeRepository;

        public TradeQueueConsumer(
            IConfiguration configuration,
            ITradeRepository tradeRepository)
        {
            _queuePath = configuration["Msmq:TradeQueuePath"];
            _tradeRepository = tradeRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var queue = new MessageQueue(_queuePath)
            {
                Formatter = new XmlMessageFormatter(new[] { typeof(TradeMessage) })
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = queue.Receive(TimeSpan.FromSeconds(5));
                    var trade = (TradeMessage)message.Body;

                    var transaction = new Transaction
                    {
                        TickerSymbol = trade.TickerSymbol,
                        Price = trade.Price,
                        Shares = trade.Shares,
                        BrokerId = trade.BrokerId,
                        Timestamp = trade.OccurredAtUtc
                    };

                    await _tradeRepository.RecordTradeAsync(
                        transaction,
                        trade.TradeValue);
                }
                catch (MessageQueueException)
                {
                    
                }
                catch (Exception)
                {
                   // log exception
                }
            }
        }
    }
}
