using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace NeuroMedia.Analytics.Agent.Parsing
{
    /// <summary>
    /// Class SHOUTcastLiveStateParser.
    /// Implements the <see cref="NeuroMedia.Analytics.Agent.LiveStateParser.BaseLiveStateParser" />
    /// Implements the <see cref="NeuroMedia.Analytics.Agent.LiveStateParser.ILiveStateParser" />
    /// </summary>
    /// <seealso cref="NeuroMedia.Analytics.Agent.LiveStateParser.BaseLiveStateParser" />
    /// <seealso cref="NeuroMedia.Analytics.Agent.LiveStateParser.ILiveStateParser" />
    [Export(typeof(ILiveStateParser))]
    public class SHOUTcastLiveStateParser : BaseLiveStateParser, ILiveStateParser
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

                var webClient = GetClientFromUri(uri);

                // Get Global Stats
                UriBuilder statsUri = new UriBuilder(uri)
                {
                    Path = "/admin.cgi",
                    Query = $"?mode=viewxml&sid={mountFilter}&pass={uri.UserInfo.Split(':')[1]}",
                    UserName = string.Empty,
                    Password = string.Empty,
                };
                var statsXml = webClient.DownloadString(statsUri.Uri);

                DateTime timestamp = DateTime.UtcNow;
                XDocument statsDoc = XDocument.Parse(statsXml);
                var mainData = statsDoc.Descendants().Where(x => x.Name.LocalName == "SHOUTCASTSERVER").First();

                LiveState liveState = new LiveState
                {
                    Status = LiveStatus.Online,
                    Timestamp = timestamp,
                    PublishingPoint = mountFilter,
                    MaxConnected = (long)mainData.Element("PEAKLISTENERS"),
                    ApiKey = apiKey,
                    Content = (string)mainData.Element("SONGTITLE")
                };

                var clients = statsDoc.Descendants().Where(x => x.Name.LocalName == "LISTENER");

                foreach (var client in clients)
                {
                    ConnectedDevice device = new ConnectedDevice()
                    {
                        IpAddress = (string)client.Element("HOSTNAME"),
                        UserAgent = (string)client.Element("USERAGENT"),
                        TimeConnected = (long)client.Element("CONNECTTIME"),
                        UserId = (string)client.Element("UID")
                    };

                    liveState.Devices.Add(device);
                }

                liveState.DeviceCount = liveState.Devices.Count;

                return new List<LiveState>() { liveState };
            }
            catch
            {
                throw;
            }
        }
    }
}
