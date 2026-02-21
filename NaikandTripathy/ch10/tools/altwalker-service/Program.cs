using System;
// Note: The ExecutorService type is provided by the AltWalker.Executor package.
// This example follows the README usage: create the service, register models and
// setups, then call Run(args) to start the executor service.

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("AltWalker ExecutorService example starting...");

        try
        {
            // This will compile if the package exposes ExecutorService in a reachable namespace.
            // If the type is not found at compile time, adjust the namespace or use the
            // fully-qualified type name as needed.
            dynamic service = Activator.CreateInstance(Type.GetType("ExecutorService, AltwalkerExecutor"));
            if (service == null)
            {
                Console.WriteLine("ExecutorService type not found via 'ExecutorService, AltwalkerExecutor'.\nIf this fails, check the actual namespace/type name in the package.");
                return;
            }

            // Example registration -- replace with your model/setup types when using.
            Console.WriteLine("Registering models (example)...");
            try { service.RegisterModel<dynamic>(); } catch { }
            try { service.RegisterSetup<dynamic>(); } catch { }

            Console.WriteLine("Starting ExecutorService.Run(args)...");
            service.Run(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to start ExecutorService: " + ex.Message);
        }

        Console.WriteLine("AltWalker ExecutorService example ended.");
    }
}
