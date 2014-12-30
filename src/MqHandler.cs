using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
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

        public MqHandler(MqConfiguration configuration, MqQueue queue, MqRecieverInvoker recieverInvoker)
        {
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
                var handlerId = i;

                Task.Factory.StartNew(() => CreateHandler(handlerId), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
            }
        }

        public void Stop()
        {
            // Stop the handlers
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private async void CreateHandler(int handlerId)
        {
            Console.WriteLine("Handler #{0}: Starting", handlerId);

            using (var messageQueue = _configuration.Creator.GetOrCreateIfNotExists(_queue.Name))
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        using (var transaction = new MessageQueueTransaction())
                        {
                            var message = messageQueue.Receive(TimeSpan.FromSeconds(3), transaction);

                            await Invoke(_queue.RecieverType, message);
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

        private async Task Invoke(Type recieverType, Message message)
        {
            if (recieverType == null)
                throw new ArgumentNullException("recieverType");

            if (message == null)
                throw new ArgumentNullException("message");

            await Task.Factory.StartNew(() => _recieverInvoker.Invoke(recieverType, message.BodyStream), TaskCreationOptions.AttachedToParent);
        }
    }
}
