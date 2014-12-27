using Newtonsoft.Json;
using System;

namespace MessageQueuer
{
    public class MqConfiguration
    {
        public Func<Type, object> Resolver { get; set; }

        public JsonSerializer Serializer { get; set; }

        public IQueueCreator Creator { get; set; }

        public MqConfiguration()
        {
            Serializer = new JsonSerializer();
            Creator = new MqQueueCreator();
        }
    }
}