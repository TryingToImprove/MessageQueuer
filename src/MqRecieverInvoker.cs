using MessageQueuer.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;

namespace MessageQueuer
{
    internal class MqRecieverInvoker
    {
        private readonly InvokerFactory _invokerFactory;
        private readonly JsonSerializer _serializer;

        internal MqRecieverInvoker(InvokerFactory invokerFactory, JsonSerializer serializer)
        {
            _invokerFactory = invokerFactory;
            _serializer = serializer;
        }

        public void Invoke(Type type, Stream messageStream)
        {
            var reciever = _invokerFactory.Build(type);
            var method = reciever.GetType().GetMethod("Invoke");
            var parameters = method.GetParameters();

            using (var reader = new BsonReader(messageStream))
            {
                var message = _serializer.Deserialize(reader, parameters[0].ParameterType);

                method.Invoke(reciever, new[] {message});
            }
        }
    }
}