using System.Messaging;

namespace MessageQueuer
{
    internal class MqQueueCreator : IQueueCreator
    {
        public MessageQueue GetOrCreateIfNotExists(string queueName)
        {
            if (MessageQueue.Exists(queueName)) return new MessageQueue(queueName);

            var queue = MessageQueue.Create(queueName, true);
            queue.UseJournalQueue = true;
            queue.Formatter = new BinaryMessageFormatter();
            queue.DefaultPropertiesToSend.Recoverable = true;

            return new MessageQueue(queueName);
        }
    }
}