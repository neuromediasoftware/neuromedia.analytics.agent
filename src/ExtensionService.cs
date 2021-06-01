using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Logging;
using NeuroMedia.Analytics.Agent.Parsing;
using NeuroMedia.Analytics.Agent.Filters;

namespace NeuroMedia.Analytics.Agent
{
    public class ExtensionService
    {
        private readonly CompositionContainer _container;
        private readonly ILogger _logger;

        public ExtensionService(ILogger<CollectorService> logger)
        {
            _logger = logger;

            // Use MEF to discover DLLs implementing additional commands and document stores
            var catalog = new AggregateCatalog();

            // Here we can add other location for the extensions
            foreach (var directory in new List<string>(){
                AppDomain.CurrentDomain.BaseDirectory, // App directory including the current DLL to avoid having to call catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
                AppDomain.CurrentDomain.BaseDirectory + "Plugins" // Subdirectory of app
                //ApplicationInfo.PluginsDirectory // Global plugin directory
            })
                if (Directory.Exists(directory))
                {
                    _logger.LogInformation("Loading parsers from {0}", directory);
                    catalog.Catalogs.Add(new DirectoryCatalog(directory, "*.dll"));
                    //catalog.Catalogs.Add(new SafeDirectoryCatalog(directory));
                }

            // Create the CompositionContainer with the parts in the catalog.
            _container = new CompositionContainer(catalog);

            // Fill the imports of this object.
            try
            {
                _logger.LogInformation("Fetching parsers");

                this._container.ComposeParts(this);

                _logger.LogInformation("{0} parsers loaded", Parsers.Count());
                _logger.LogInformation("{0} filters loaded", Filters.Count());
            }
            catch (CompositionException compositionException)
            {
                _logger.LogError(compositionException, "Failed to load parsers: " + compositionException.Message);
                throw;
            }
        }

        // Parsing
        public ILiveStateParser GetParser(string typeName)
        {
            return Parsers.First(p => p.GetType().Name == typeName);
        }

        [ImportMany]
        public IEnumerable<ILiveStateParser> Parsers { get; private set; }

        // Filtering
        public ILiveStateFilter GetFilter(string typeName)
        {
            return Filters.First(p => p.GetType().Name == typeName);
        }

        [ImportMany]
        public IEnumerable<ILiveStateFilter> Filters { get; private set; }
    }
}
