using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using System.IO;
using System;
using System.Threading;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace NeuroMedia.Analytics.Agent
{
    class Program
    {
        public static void Main(string[] args)
        {
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);

            UseOnlyInvariantCulture();
            SetCurrentDirectoryToAppPath();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .UseWindowsService()
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File($"Logs{Path.DirectorySeparatorChar}NeuroMedia.Analytics.Agent-.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", retainedFileCountLimit: 15))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();

                    if (args != null && args.Length > 0)
                    {
                        config.AddCommandLine(args);
                    }
                    else
                    {
                        config.SetBasePath(GetBasePath());
                        config.AddJsonFile("appsettings.json");
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHangfire(configuration => configuration
                                //.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                                //.UseSimpleAssemblyNameTypeSerializer()
                                //.UseRecommendedSerializerSettings()
                                .UseMemoryStorage());
                    services.AddHangfireServer();

                    GlobalConfiguration.Configuration.UseMemoryStorage().UseSerilogLogProvider();

                    services.AddOptions();
                    services.Configure<AgentConfig>(hostContext.Configuration.GetSection("agentConfig"));

                    services.AddSingleton<LiveStateService>();
                    services.AddSingleton<ExtensionService>();
                    services.AddHostedService<CollectorService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

        private static void UseOnlyInvariantCulture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        private static void SetCurrentDirectoryToAppPath()
        {
            try
            {
                Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Error setting current directory: {directory}", AppContext.BaseDirectory);
            }
        }

        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }
    }
}
