using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeuroMedia.Analytics.Agent.Parsing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMedia.Analytics.Agent
{
    public class AgentService
    {
        private readonly string[] _arguments;
        public AgentService(string[] args)
        {
            _arguments = args;
        }

        public async Task Start()
        {
            var builder = new HostBuilder()
                        .UseSystemd()
                        .UseWindowsService()
                        .UseSerilog((context, services, configuration) => configuration
                            .ReadFrom.Configuration(context.Configuration)
                            .ReadFrom.Services(services)
                            .Enrich.FromLogContext()
                            .WriteTo.Console())
                        .ConfigureAppConfiguration((hostingContext, config) =>
                        {
                            config.AddEnvironmentVariables();

                            if (_arguments != null && _arguments.Length > 0)
                            {
                                config.AddCommandLine(_arguments);
                            }
                            else
                            {
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
                            services.AddSingleton<IHostedService, CollectorService>();
                            services.AddSingleton<ExtensionService>();
                        })
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                            logging.AddConsole();
                        });

            await builder.RunConsoleAsync();
        }

        internal void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
