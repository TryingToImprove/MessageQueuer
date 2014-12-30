using MessageQueuer.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueuer
{
    public class MqRunner
    {
        private readonly MqConfiguration _configuration;
        private readonly MqRecieverInvoker _recieverInvoker;
        private readonly TypeLocator _typeLocator;

        private readonly ConcurrentBag<MqQueue> _queues = new ConcurrentBag<MqQueue>();
        private readonly ConcurrentBag<MqHandler> _handlers = new ConcurrentBag<MqHandler>();

        private bool _isInitialized;

        public MqRunner(MqConfiguration configuration)
        {
            _configuration = configuration;
            _typeLocator = new TypeLocator();
            _recieverInvoker = new MqRecieverInvoker(new InvokerFactory(configuration.Resolver), configuration.Serializer);
        }

        public void Start()
        {
            Start(null);
        }

        public void Start(Action<Exception> onException)
        {
            if (!_isInitialized)
                Initialize();

            Task.Factory.StartNew(() => Parallel.ForEach(_queues, queue =>
            {
                var handler = new MqHandler(_configuration, queue, _recieverInvoker);
                handler.Start();

                _handlers.Add(handler);
            }), TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            foreach (var handler in _handlers)
            {
                handler.Stop();
            }
        }

        private void Initialize()
        {
            foreach (var reciever in _typeLocator.Locate<MqRecieverAttribute>())
            {
                var queueAttributes = reciever.Attributes
                    .Where(x => x.GetType() == typeof(MqRecieverAttribute))
                    .Cast<MqRecieverAttribute>()
                    .ToArray();

                if (queueAttributes.Count() > 1)
                    throw new InvalidOperationException(string.Format("Multiple MqRecieverAttribute was found on {0}", reciever.Type.Name));

                var queueAttribute = queueAttributes.First();

                _queues.Add(new MqQueue()
                {
                    Name = queueAttribute.Name,
                    RecieverType = reciever.Type,
                    Handlers = queueAttribute.Handlers
                });
            }

            _isInitialized = true;
        }
    }
}