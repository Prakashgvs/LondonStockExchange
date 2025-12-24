using Core.Entities;
using Core.Interfaces;
using MSMQ.Messaging;

namespace Services.Messaging
{
    public class MsmqTradeQueue : ITradeQueue
    {
        private readonly string _queuePath;

        public MsmqTradeQueue(IConfiguration configuration)
        {
            _queuePath = configuration["Msmq:TradeQueuePath"];

            if (string.IsNullOrWhiteSpace(_queuePath))
            {
                throw new InvalidOperationException("MSMQ TradeQueuePath is not configured.");
            }

            EnsureQueueExists();
        }

        public Task EnqueueAsync(TradeMessage tradeMessage, CancellationToken cancellationToken)
        {
            if (tradeMessage == null)
                throw new ArgumentNullException(nameof(tradeMessage));

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
            }
            catch
            {
                transaction.Abort();
                throw;
            }

            return Task.CompletedTask;
        }

        private void EnsureQueueExists()
        {
            if (!MessageQueue.Exists(_queuePath))
            {
                MessageQueue.Create(_queuePath, transactional: true);
            }
        }
    }
}