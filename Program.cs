using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp8Hosting
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .UseServiceProviderFactory(new AspectCoreServiceProviderFactory())  //注销正常 Process is terminating due to StackOverflowException.
                .ConfigureHostConfiguration(conf =>
                {
                    conf
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);

                })
                .ConfigureAppConfiguration((context, conf) =>
                {
                    var env = context.HostingEnvironment;
                    conf
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((context, logging) => {
                    logging
                        .AddConfiguration(context.Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug();
                })
                .ConfigureServices(serviceColl => {
                    serviceColl.AddHostedService<ThriftHost>();
                })
                .Build();

            var log = host.Services.GetRequiredService<ILogger<Program>>();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Console.WriteLine($"UnhandledException,app is terminating:{e.IsTerminating} exception:{e.ExceptionObject}");
                log.LogError(e.ExceptionObject as Exception, $"UnhandledException,app is terminating:{e.IsTerminating}");
            };

            host.StartAsync();
            Console.ReadKey();
            host.StopAsync();
            host.Dispose();

            Console.WriteLine("end");
        }
    }

    public class ThriftHost : Microsoft.Extensions.Hosting.BackgroundService
    {
        private ILogger<ThriftHost> _log;

        public ThriftHost(ILogger<ThriftHost> log, IConfiguration config)
        {
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation("ThriftHost start...");
            await Task.CompletedTask;

            //await ServerStartup.Init<Thrift.UserManage.IAsync, Fanews.UserManage.Thrift.UserManage.AsyncProcessor>(_thriftServerConfig, _thriftService, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("ThriftHost stop...");
            //await Task.CompletedTask;

            //await ServerStartup.Stop(_thriftServerConfig, _log, cancellationToken);
            await Task.Delay(1000, cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
