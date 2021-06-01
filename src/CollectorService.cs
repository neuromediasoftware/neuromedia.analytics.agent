using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeuroMedia.Analytics.Agent.Parsing;
using NeuroMedia.Analytics.LiveWatch.Models;
using Newtonsoft.Json;
using RestSharp;

namespace NeuroMedia.Analytics.Agent
{

    /// <summary>Class CollectorService.
    /// Implements the <see cref="Microsoft.Extensions.Hosting.BackgroundService" /></summary>
    public class CollectorService : BackgroundService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IOptions<AgentConfig> _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="liveStateService">The live state service.</param>
        public CollectorService(ILogger<CollectorService> logger, IOptions<AgentConfig> config)
        {
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// start as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting agent: " + _config.Value.Name);

            BackgroundJob.Enqueue<LiveStateService>(x => x.ProcessSources());
            RecurringJob.AddOrUpdate<LiveStateService>("LiveState", x => x.ProcessSources(), "* * * * *");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000 * 60 * 15);
                _logger.LogInformation("Ping");
            }
        }
    }
}
