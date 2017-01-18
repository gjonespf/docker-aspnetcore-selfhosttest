using System;
using PeterKottas.DotNetCore.WindowsService;

namespace SelfHostTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceRunner<ExampleService>.Run(config =>
            {
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments) =>
                    {
                        return new ExampleService();
                    });

                    serviceConfig.OnStart((service, extraParams) =>
                    {
                        Console.WriteLine("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
            });
            if (!Console.IsInputRedirected)
            {
                Console.ReadKey();
            }


        }
    }
}
