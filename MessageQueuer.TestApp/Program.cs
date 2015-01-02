using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MessageQueuer.TestApp.Messages;
using Ninject;

namespace MessageQueuer.TestApp
{
    class Program
    {
        public static void Main(string[] args)
        {
            // Using ninject as IoC-container
            var kernel = new StandardKernel();

            // Setup configuraiton
            var configuration = new MqConfiguration
            {
                // Setup method for constructor injection for the recievers
                Resolver = (type) => kernel.Get(type)
            };

            // Create a instance of the runner
            var runner = new MqRunner(configuration);

            // Start the runner
            runner.Start((exception) =>
            {
                Console.WriteLine("There was a exception!");

                Console.WriteLine(exception.Message);
                if (exception.InnerException != null) Console.WriteLine(exception.InnerException.Message);
                if (exception.InnerException != null && exception.InnerException.InnerException != null) Console.WriteLine(exception.InnerException.InnerException.Message);
                //Environment.Exit(0);
            });

            Console.WriteLine("Program: Thread #{0}", Thread.CurrentThread.ManagedThreadId);

            var sender = new MqSender(configuration);
            var random = new Random();

            // If running in console application, then keep the app running
            var lastCommand = string.Empty;
            while (true)
            {
                Console.WriteLine("Enter command: ");

                lastCommand = Console.ReadLine();

                if (lastCommand == null)
                    continue;

                if (lastCommand.Equals("Add", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.Write("Enter your name: ");
                    lastCommand = Console.ReadLine();

                    sender.Send(Queues.HelloWorld, new HelloWorldMessage
                    {
                        Name = lastCommand
                    });

                    continue;
                }

                if (lastCommand.Equals("Add -multiple", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.Write("Enter number of messages: ");
                    lastCommand = Console.ReadLine();

                    Parallel.For(0, int.Parse(lastCommand), i => sender.Send(Queues.HelloWorld, new HelloWorldMessage
                    {
                        Name = "Name" + random.Next()
                    }));

                    Console.WriteLine("Added");

                    continue;
                }

                if (lastCommand.Equals("Stop", StringComparison.InvariantCultureIgnoreCase))
                {
                    runner.Stop();
                    continue;
                }

                if (lastCommand.Equals("Start", StringComparison.InvariantCultureIgnoreCase))
                {
                    runner.Start();
                    continue;
                }

                if (lastCommand.Equals("close", StringComparison.InvariantCultureIgnoreCase))
                {
                    Environment.Exit(0);
                    continue;
                }
            }
        }
    }
}
