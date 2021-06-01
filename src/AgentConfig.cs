using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroMedia.Analytics.Agent
{
    public class AgentConfig
    {
        public string Name { get; set; }
        public string ApiKey { get; set; }
        public string ConfigFile { get; set; }
        public string Endpoint { get; set; }
        public List<AgentConfigSource> Sources { get; set; }
    }

    public class AgentConfigSource
    {
        public string Name { get; set; }
        public string ParserName { get; set; }
        public string Uri { get; set; }
        public int Frequency { get; set; }
        public IList<string> Filters { get; set; }
    }
}
