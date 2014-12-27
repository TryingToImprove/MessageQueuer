MessageQueuer
=============

A project which make it simpler to use MSMQ

- Nuget (https://www.nuget.org/packages/MessageQueuer/0.1.0)


### Using the library

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
        Environment.Exit(0);
    });

    // If running in console application, then keep the app running
    Console.ReadKey();
    
    
### Recievers
    [MqReciever(Name = ".\\PRIVATE$\\console-output", Handlers = 2)]
    public class ConsoleOutputMessageReciever : IMqReciever<OutputMessage>
    {
        public void Invoke(OutputMessage message)
        {
            Console.WriteLine(message.Text);
        }
    }

    public class OutputMessage
    {
        public string Text { get; set; }
    }
    
This MqRunner will automatically scan the assemblies for `MqReciever`'s

### More info
Please contact me or open a issue..
