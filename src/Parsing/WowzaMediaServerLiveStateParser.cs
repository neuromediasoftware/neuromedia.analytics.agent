using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NeuroMedia.Analytics.Agent.Parsing
{
    [Export(typeof(ILiveStateParser))]
    public class WowzaMediaServerLiveStateParser : BaseLiveStateParser, ILiveStateParser
    {
        /// <summary>
        /// Gets the state of the live.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>IList&lt;LiveState&gt;.</returns>
        public IList<LiveState> GetLiveState(Uri uri, string apiKey)
        {
            try
            {
                string mountFilter = uri.AbsolutePath.TrimStart('/');

                string[] filterParts = mountFilter.Split('/');

                var webClient = GetClientFromUri(uri);

                // Get Global Stats
                UriBuilder statsUri = new UriBuilder(uri)
                {
                    Path = "/serverinfo"
                };
                var statsXml = webClient.DownloadString(statsUri.Uri);

                DateTime timestamp = DateTime.UtcNow;
                XDocument statsDoc = XDocument.Parse(statsXml);

                IList<LiveState> liveStates = new List<LiveState>();
                var vhosts = statsDoc.Descendants().Where(x => x.Name.LocalName == "VHost");

                foreach(var vhost in vhosts)
                {
                    var vhostName = (string)vhost.Element("Name");

                    // Filter if publishing point information is provided
                    if (filterParts.Length > 0 && filterParts[0] != vhostName)
                        continue;

                    var applications = statsDoc.Descendants().Where(x => x.Name.LocalName == "Application");

                    foreach (var application in applications)
                    {
                        var applicationName = (string)application.Element("Name");

                        // Filter if publishing point information is provided
                        if (filterParts.Length > 1 && filterParts[1] != applicationName)
                            continue;

                        var applicationInstances = statsDoc.Descendants().Where(x => x.Name.LocalName == "ApplicationInstance");

                        foreach(var applicationInstance in applicationInstances)
                        {
                            var applicationInstanceName = (string)applicationInstance.Element("Name");

                            // Filter if publishing point information is provided
                            if (filterParts.Length > 2 && filterParts[2] != applicationInstanceName)
                                continue;

                            var clients = statsDoc.Descendants().Where(x => x.Name.LocalName == "ApplicationInstance");

                            LiveState liveState = new LiveState
                            {
                                Status = LiveStatus.Online,
                                Timestamp = timestamp,
                                PublishingPoint = $"{vhostName}/{applicationName}/{applicationInstanceName}",
                                ApiKey = apiKey
                            };

                            foreach (var client in clients)
                            {
                                ConnectedDevice device = new ConnectedDevice()
                                {
                                    IpAddress = (string)client.Element("IpAddress"),
                                    UserAgent = (string)client.Element("UserAgent"),
                                    TimeConnected = (long)client.Element("TimeRunning"),
                                    UserId = (string)client.Element("ClientId"),
                                    Referrer = (string)client.Element("Referrer")
                                };

                                liveState.Devices.Add(device);
                            }

                            liveState.DeviceCount = liveState.Devices.Count;

                            liveStates.Add(liveState);
                        }
                    }
                }

                return liveStates;
            }
            catch
            {
                throw;
            }
        }
    }
}
