using System.Messaging;

namespace MessageQueuer
{
    public interface IQueueCreator
    {
        MessageQueue GetOrCreateIfNotExists(string queueName);
    }
}