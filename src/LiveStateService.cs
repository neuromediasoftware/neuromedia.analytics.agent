using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeuroMedia.Analytics.Agent.Parsing;
using NeuroMedia.Analytics.LiveWatch.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeuroMedia.Analytics.Agent
{
    /// <summary>
    /// Class LiveStateService.
    /// </summary>
    public class LiveStateService
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly AgentConfig _configuration;
        /// <summary>
        /// The parser service
        /// </summary>
        private readonly ExtensionService _extensionService;
        /// <summary>
        /// The state
        /// </summary>
        private readonly IDictionary<string, DateTime> _state = new Dictionary<string, DateTime>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveStateService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="extensionService">The parser service.</param>
        public LiveStateService(ILogger<CollectorService> logger, IOptions<AgentConfig> config, ExtensionService extensionService)
        {
            _logger = logger;
            _extensionService = extensionService;

            _configuration = (string.IsNullOrEmpty(config.Value.ConfigFile) ? config.Value : JsonConvert.DeserializeObject<AgentConfig>(File.ReadAllText(config.Value.ConfigFile)));

            foreach (var source in _configuration.Sources)
                if (!_state.ContainsKey(source.Name))
                    _state.Add(source.Name, DateTime.UtcNow);
        }

        /// <summary>
        /// Processes the sources.
        /// </summary>
        public void ProcessSources()
        {
            foreach (var source in _configuration.Sources)
            {
                if (_state[source.Name] < DateTime.UtcNow)
                {
                    BackgroundJob.Enqueue<LiveStateService>(x => x.GetLiveState(source, source.ParserName, null, CancellationToken.None));
                    _state[source.Name] = DateTime.UtcNow.AddMinutes(source.Frequency);
                    _logger.LogInformation("Queued job for {0}", source.Name);
                }
            }
        }

        /// <summary>
        /// Gets the state of the live.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="parserName">Name of the parser.</param>
        /// <param name="context">The context.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        [AutomaticRetry(Attempts = 3)]
        //[Queue("live")]
        [DisplayName("Live state for #{0}")]
        public async Task GetLiveState(AgentConfigSource source, string parserName, PerformContext context, CancellationToken cancellationToken)
        {
            Stopwatch watch = Stopwatch.StartNew();

            IList<LiveState> liveStates = GetLiveStates(new Uri(source.Uri), parserName, _configuration.ApiKey);
            long connectedDevices = liveStates.Select(x => x.DeviceCount).Sum();
            _logger.LogInformation("Retrieved {0} states for {1} containing {2} connected devices", liveStates.Count, source.Name, connectedDevices);

            foreach(var filterName in source.Filters)
            {
                var filter = _extensionService.GetFilter(filterName);
                filter.OnParsed(liveStates);
            }
            _logger.LogInformation("Filtered {0} states for {1} containing {2} connected devices", liveStates.Count, source.Name, connectedDevices);

            await PostLiveState(liveStates);
            _logger.LogInformation("Stored {0} states for {1} containing {2} connected devices ({3})", liveStates.Count, source.Name, connectedDevices, watch.Elapsed);
        }

        /// <summary>
        /// Gets the live states.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="parserName">Name of the parser.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>IList&lt;LiveState&gt;.</returns>
        private IList<LiveState> GetLiveStates(Uri uri, string parserName, string apiKey)
        {
            return _extensionService.GetParser(parserName).GetLiveState(uri, apiKey);
        }

        /// <summary>
        /// Posts the state of the live.
        /// </summary>
        /// <param name="liveStates">The live states.</param>
        private async Task PostLiveState(IList<LiveState> liveStates)
        {
            try
            {
                var client = new RestClient(_configuration.Endpoint);

                var request = new RestRequest("/LiveWatch", Method.POST, DataFormat.Json);
                request.AddJsonBody(JsonConvert.SerializeObject(liveStates));
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new ApplicationException(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
