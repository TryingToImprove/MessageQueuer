using System;
using System.Threading;
using MessageQueuer.TestApp.Messages;

namespace MessageQueuer.TestApp.Recievers
{
    [MqReciever(Name = Queues.HelloWorld, Handlers = 3)]
    class HelleWorldMessageReciever : IMqReciever<HelloWorldMessage>
    {
        private static int I;

        public void Invoke(HelloWorldMessage message)
        {
            I++;

            if (I > 21)
            {
                throw new Exception("TEST");
            }

            var currentType = GetType();

            Console.WriteLine("{0}: Thread #{1}", currentType.Name, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("{0}: {1}", currentType.Name, message.Name);
        }
    }
}
