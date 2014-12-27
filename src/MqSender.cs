using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.IO;
using System.Messaging;

namespace MessageQueuer
{
    public class MqSender
    {
        private readonly IQueueCreator _queueCreator;
        private readonly JsonSerializer _serializer;

        public MqSender(MqConfiguration configuration)
        {
            _queueCreator = configuration.Creator;
            _serializer = configuration.Serializer;
        }

        public void Send(string queueName, object message)
        {
            using (var messageQueue = _queueCreator.GetOrCreateIfNotExists(queueName))
            using (var stream = new MemoryStream())
            using (var writer = new BsonWriter(stream))
            {
                _serializer.Serialize(writer, message);

                using (var msg = new Message())
                {
                    msg.BodyStream = stream;

                    messageQueue.Send(msg);
                }
            }
        }
    }
}