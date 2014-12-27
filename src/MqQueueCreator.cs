using System.Messaging;

namespace MessageQueuer
{
    internal class MqQueueCreator : IQueueCreator
    {
        public MessageQueue GetOrCreateIfNotExists(string queueName)
        {
            if (!MessageQueue.Exists(queueName))
            {
                MessageQueue.Create(queueName);
            }

            return new MessageQueue(queueName) {UseJournalQueue = true, Formatter = new BinaryMessageFormatter()};
        }
    }
}