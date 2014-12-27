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
        private readonly IQueueCreator _queueCreator;
        private readonly IDictionary<string, Type> _recieverDictionary = new ConcurrentDictionary<string, Type>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private bool _isInitialized;

        public MqRunner(MqConfiguration configuration)
        {
            _configuration = configuration;
            _typeLocator = new TypeLocator();
            _queueCreator = configuration.Creator;
            _recieverInvoker = new MqRecieverInvoker(new InvokerFactory(configuration.Resolver),configuration.Serializer);
        }

        public void Start()
        {
            Start(null);
        }

        public void Start(Action<Exception> onException)
        {
            if (!_isInitialized)
                Initialize();

            Parallel.ForEach(_recieverDictionary, reciever =>
                Task.Factory.StartNew(() =>
                {
                    using (var messageQueue = _queueCreator.GetOrCreateIfNotExists(reciever.Key))
                    {
                        Execute(reciever, messageQueue);
                    }
                }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current)
                    .ContinueWith(x =>
                    {
                        if (onException != null)
                            onException.Invoke(x.Exception);

                        Stop();
                    }, TaskContinuationOptions.OnlyOnFaulted));
        }

        private void Execute(KeyValuePair<string, Type> reciever1, MessageQueue messageQueue)
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                RecieveMessage(reciever1, messageQueue);
            }
        }

        private void RecieveMessage(KeyValuePair<string, Type> reciever, MessageQueue messageQueue)
        {
            try
            {
                using (var transaction = new MessageQueueTransaction())
                {
                    var message = messageQueue.Receive(TimeSpan.FromSeconds(3), transaction);

                    Invoke(reciever, message);
                }
            }
            catch (MessageQueueException ex)
            {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                    throw;
            }
        }

        private void Invoke(KeyValuePair<string, Type> reciever, Message message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            _recieverInvoker.Invoke(reciever.Value, message.BodyStream);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            Console.WriteLine("STOP");
        }

        private void Initialize()
        {
            var recievers = _typeLocator.Locate<MqRecieverAttribute>();

            foreach (var reciever in recievers.Select(x => new {Attribute = x.Attributes.First(), Type = x.Type}))
            {
                _recieverDictionary.Add(reciever.Attribute.Name, reciever.Type);
            }

            _isInitialized = true;
        }
    }

    public class MqRecieverAttribute : Attribute
    {
        public string Name { get; set; }

        public int Handlers { get; set; }
    }
}