using Core.Entities;
using Core.Interfaces;
using MSMQ.Messaging;

namespace Services.Messaging
{
    public class MsmqTradeQueue : ITradeQueue
    {
        private readonly string _queuePath;
        private readonly IApplicationLogger _logger;

        public MsmqTradeQueue(IConfiguration configuration, IApplicationLogger logger)
        {
            _logger = logger;
            _queuePath = configuration["Msmq:TradeQueuePath"];

            if (string.IsNullOrWhiteSpace(_queuePath))
            {
                _logger.LogError("MSMQ TradeQueuePath is not configured");
                throw new InvalidOperationException("MSMQ TradeQueuePath is not configured.");
            }

            _logger.LogInformation("Initializing MSMQ Trade Queue - Path: {QueuePath}", _queuePath);

            EnsureQueueExists();
        }

        public Task EnqueueAsync(TradeMessage tradeMessage, CancellationToken cancellationToken)
        {
            if (tradeMessage == null)
            {
                _logger.LogWarning("Attempted to enqueue null trade message");
                throw new ArgumentNullException(nameof(tradeMessage));
            }

            _logger.LogDebug("Enqueuing trade message - TradeId: {TradeId}, Ticker: {Ticker}, BrokerId: {BrokerId}",
                tradeMessage.TradeId, tradeMessage.TickerSymbol, tradeMessage.BrokerId);

            using var queue = new MessageQueue(_queuePath)
            {
                Formatter = new XmlMessageFormatter(new[] { typeof(TradeMessage) })
            };

            using var transaction = new MessageQueueTransaction();
            transaction.Begin();

            try
            {
                queue.Send(tradeMessage, transaction);
                transaction.Commit();

                _logger.LogInformation("Trade message enqueued successfully - TradeId: {TradeId}, Ticker: {Ticker}, TradeValue: {TradeValue}",
                    tradeMessage.TradeId, tradeMessage.TickerSymbol, tradeMessage.TradeValue);
            }
            catch (Exception ex)
            {
                transaction.Abort();

                _logger.LogError("Failed to enqueue trade message - TradeId: {TradeId}, Ticker: {Ticker}",
                    ex, tradeMessage.TradeId, tradeMessage.TickerSymbol);

                throw;
            }

            return Task.CompletedTask;
        }

        private void EnsureQueueExists()
        {
            try
            {
                if (!MessageQueue.Exists(_queuePath))
                {
                    _logger.LogWarning("Queue does not exist, creating new queue - Path: {QueuePath}", _queuePath);

                    MessageQueue.Create(_queuePath, transactional: true);

                    _logger.LogInformation("Queue created successfully - Path: {QueuePath}", _queuePath);
                }
                else
                {
                    _logger.LogInformation("Queue already exists - Path: {QueuePath}", _queuePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to ensure queue exists - Path: {QueuePath}", ex, _queuePath);
                throw;
            }
        }
    }
}