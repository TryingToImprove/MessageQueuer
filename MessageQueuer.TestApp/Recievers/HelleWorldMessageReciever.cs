using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageQueuer.TestApp.Messages;

namespace MessageQueuer.TestApp.Recievers
{
    [MqReciever(Name = Queues.HelloWorld, Handlers = 3 )]
    class HelleWorldMessageReciever : IMqReciever<HelloWorldMessage>
    {
        public void Invoke(HelloWorldMessage message)
        {
            var currentType = GetType();

            Console.WriteLine("{0}: Thread #{1}", currentType.Name, Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine("{0}: {1}", currentType.Name, message.Name);
        }
    }
}
