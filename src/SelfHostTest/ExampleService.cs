using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PeterKottas.DotNetCore.WindowsService.Base;
using PeterKottas.DotNetCore.WindowsService.Interfaces;


namespace SelfHostTest
{
    public class ExampleService : MicroService, IMicroService
    {
        IConfigurationRoot Configuration;

        public void Start()
        {
            WriteLog("Service started");
            this.StartBase();
            Configure();
            TimerStartup();
            KestrelStartup();
        }

        private void Configure()
        {
            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(env))
                env = "Development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("hosting.json", optional: true, reloadOnChange: true);
            builder.AddUserSecrets();
            Configuration = builder.Build();
        }

        private void TimerStartup()
        {
            Timers.Start("Poller", 1000, () =>
                {
                    var now = DateTime.Now;
                    WriteLog($"Polling at {now:o}\n");
                },
                (e) =>
                {
                    WriteLog($"Exception while polling: {e}\n");
                }
            );
        }

        private void KestrelStartup()
        {
            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var directoryPath = System.IO.Path.GetDirectoryName(exePath);
            var croot = directoryPath;
            bool useSSL = false;
            if(true)
                croot = System.IO.Directory.GetCurrentDirectory();

            // Check cert setup
            var certpath = Configuration["server.https.cert"];
            if(!string.IsNullOrEmpty(certpath))
            {
                if (!System.IO.File.Exists(certpath))
                {
                    WriteLog($"Invalid SSL certpath: '{certpath}'");
                }
                else
                {
                    WriteLog($"Using SSL certpath: '{certpath}'");
                    useSSL = true;
                }
            }

            IWebHost host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    if (useSSL)
                    {
                        options.UseHttps(Configuration["server.https.cert"], Configuration["server.https.cert.password"]);
                    }
                })
                .UseContentRoot(directoryPath)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseContentRoot(croot)
                .UseUrls(Configuration["server.urls"])
                .Build();
            host.Run();
        }

        public void Stop()
        {
            this.StopBase();
            WriteLog("Service stopped");
        }


        private void WriteLog(string v)
        {
            Console.WriteLine(v);
        }
    }
}
