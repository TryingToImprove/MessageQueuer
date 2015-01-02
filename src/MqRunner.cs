using MessageQueuer.Core;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueuer
{
    public class MqRunner
    {
        private static readonly object Lock = new object();

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

            Task.Factory.StartNew(() =>
            {
                Parallel.ForEach(_queues, (queue, state) =>
                {
                    var handler = new MqHandler(_configuration, queue, _recieverInvoker);
                    handler.Start();

                    _handlers.Add(handler);
                });
            }, TaskCreationOptions.LongRunning)
            .ContinueWith(x =>
            {
                lock (Lock)
                {
                    if (!x.IsFaulted) return;

                    // Lets stop the runner
                    Stop();

                    // Invoke the onException method
                    if (x.Exception != null)
                    {
                        var flatException = x.Exception.Flatten();

                        if (flatException.InnerException != null && flatException.InnerException.InnerException != null)
                            onException.Invoke(flatException.InnerException.InnerException);
                        else if (flatException.InnerException != null)
                            onException.Invoke(flatException.InnerException);
                        else
                            onException.Invoke(flatException);
                    }
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void Stop()
        {
            while (!_handlers.IsEmpty)
            {
                MqHandler handler;

                if (!_handlers.TryTake(out handler))
                {
                    break;
                }

                if (handler.IsRunning)
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

                _queues.Add(new MqQueue
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