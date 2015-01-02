using System;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueuer
{
    internal class MqHandler
    {
        private readonly MqConfiguration _configuration;
        private readonly MqQueue _queue;
        private readonly MqRecieverInvoker _recieverInvoker;

        private CancellationTokenSource _cancellationTokenSource;

        private static readonly object Lock = new object();

        public bool IsRunning { get; private set; }

        public MqHandler(MqConfiguration configuration, MqQueue queue, MqRecieverInvoker recieverInvoker)
        {
            IsRunning = false;

            _configuration = configuration;
            _queue = queue;
            _recieverInvoker = recieverInvoker;
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // Begin task with a single handler
            for (var i = 0; i < _queue.Handlers; i++)
            {
                Task.Factory.StartNew(CreateHandler, _cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }

            IsRunning = true;
        }

        public void Stop()
        {
            lock (Lock)
            {
                // Stop the handlers
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();

                IsRunning = false;
            }
        }

        private void CreateHandler()
        {
            using (var messageQueue = GetQueue(_queue.Name))
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        using (var tx = new MessageQueueTransaction())
                        {
                            tx.Begin();

                            var message = messageQueue.Receive(TimeSpan.FromSeconds(3), tx);

                            Invoke(_queue.RecieverType, message);

                            tx.Commit();
                        }
                    }
                    catch (MessageQueueException ex)
                    {
                        if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                            throw;
                    }
                }
            }
        }

        private MessageQueue GetQueue(string queueName)
        {
            lock (Lock)
            {
                return _configuration.Creator.GetOrCreateIfNotExists(queueName);
            }
        }

        private void Invoke(Type recieverType, Message message)
        {
            if (recieverType == null)
                throw new ArgumentNullException("recieverType");

            if (message == null)
                throw new ArgumentNullException("message");

            Task.Factory.StartNew(() => _recieverInvoker.Invoke(recieverType, message.BodyStream)).Wait();
        }
    }
}
