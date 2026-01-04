using Core.Entities;
using Core.Interfaces;
using MSMQ.Messaging;
using Microsoft.Extensions.Configuration;

namespace Services.Messaging
{
    public class TradeQueueConsumer : BackgroundService
    {
        private readonly string _queuePath;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationLogger _logger;

        public TradeQueueConsumer(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IApplicationLogger logger)
        {
            _queuePath = configuration["Msmq:TradeQueuePath"];
            _serviceProvider = serviceProvider;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_queuePath))
            {
                _logger.LogError("MSMQ TradeQueuePath is not configured for consumer");
                throw new InvalidOperationException("MSMQ TradeQueuePath is not configured.");
            }

            _logger.LogInformation("Trade Queue Consumer initialized - QueuePath: {QueuePath}", _queuePath);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trade Queue Consumer started");

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

                    _logger.LogInformation("Trade message received from queue - TradeId: {TradeId}, Ticker: {Ticker}, BrokerId: {BrokerId}",
                        trade.TradeId, trade.TickerSymbol, trade.BrokerId);

                    await ProcessTradeAsync(trade);

                    _logger.LogInformation("Trade processed successfully - TradeId: {TradeId}", trade.TradeId);
                }
                catch (MessageQueueException mqEx) when (mqEx.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                {
                    _logger.LogDebug("Queue receive timeout - no messages available");
                    await Task.Delay(100, stoppingToken);
                }
                catch (MessageQueueException mqEx)
                {
                    _logger.LogError("MSMQ error occurred - ErrorCode: {ErrorCode}", mqEx, mqEx.MessageQueueErrorCode);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (InvalidCastException icEx)
                {
                    _logger.LogError("Failed to deserialize trade message from queue", icEx);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unexpected error processing trade from queue", ex);
                    await Task.Delay(1000, stoppingToken);
                }
            }

            _logger.LogInformation("Trade Queue Consumer stopped");
        }

        private async Task ProcessTradeAsync(TradeMessage trade)
        {
            using var scope = _serviceProvider.CreateScope();
            var tradeRepository = scope.ServiceProvider.GetRequiredService<ITradeRepository>();

            _logger.LogDebug("Processing trade - TradeId: {TradeId}, Ticker: {Ticker}, Price: {Price}, Shares: {Shares}, TradeValue: {TradeValue}",
                trade.TradeId, trade.TickerSymbol, trade.Price, trade.Shares, trade.TradeValue);

            var transaction = new Transaction
            {
                TradeId = trade.TradeId,
                TickerSymbol = trade.TickerSymbol,
                Price = trade.Price,
                Shares = trade.Shares,
                BrokerId = trade.BrokerId,
                Timestamp = trade.OccurredAtUtc
            };

            try
            {
                var transactionId = await tradeRepository.RecordTradeAsync(transaction, trade.TradeValue);

                _logger.LogInformation("Trade persisted to database - TradeId: {TradeId}, TransactionId: {TransactionId}, Ticker: {Ticker}",
                    trade.TradeId, transactionId, trade.TickerSymbol);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to persist trade to database - TradeId: {TradeId}, Ticker: {Ticker}",
                    ex, trade.TradeId, trade.TickerSymbol);
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Trade Queue Consumer is stopping");
            return base.StopAsync(cancellationToken);
        }
    }
}
