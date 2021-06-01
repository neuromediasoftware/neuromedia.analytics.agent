using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ComponentModel.Composition;

namespace NeuroMedia.Analytics.Agent.Parsing
{
    [Export(typeof(ILiveStateParser))]
    public class IcecastLiveStateParser : BaseLiveStateParser, ILiveStateParser
    {
        public IList<LiveState> GetLiveState(Uri uri, string apiKey)
        {
            string mountFilter = uri.AbsolutePath.TrimStart('/');
            string mountQuery = (string.IsNullOrWhiteSpace(mountFilter) ? string.Empty : "?mount=/" + mountFilter);

            var webClient = GetClientFromUri(uri);

            // Get Global Stats
            UriBuilder statsUri = new UriBuilder(uri)
            {
                Path = "admin/stats",
                Query = mountQuery
            };
            var statsXml = webClient.DownloadString(statsUri.Uri);

            XDocument statsDoc = XDocument.Parse(statsXml);
            var mounts = statsDoc.Descendants().Where(x => x.Name.LocalName == "source");

            // Get Clients for each mount
            IList<LiveState> liveStates = new List<LiveState>();

            foreach (var mount in mounts)
            {
                string mountName = mount.Attribute("mount").Value;

                UriBuilder listClientsUri = new UriBuilder(uri)
                {
                    Path = "admin/listclients",
                    Query = "?mount=" + mountName
                };

                DateTime timestamp = DateTime.UtcNow;
                var clientsXml = webClient.DownloadString(listClientsUri.Uri);

                XDocument clientDocs = XDocument.Parse(clientsXml);
                var clients = clientDocs.Descendants().Where(x => x.Name.LocalName == "listener");

                LiveState liveState = new LiveState
                {
                    Status = LiveStatus.Online,
                    Timestamp = timestamp,
                    PublishingPoint = mountName.TrimStart('/'),
                    MaxConnected = (long)mount.Element("listener_peak"),
                    ApiKey = apiKey,
                    Content = (string)mount.Element("title")
                };

                foreach (var client in clients)
                {
                    ConnectedDevice device = new ConnectedDevice()
                    {
                        IpAddress = (string)client.Element("IP"),
                        UserAgent = (string)client.Element("UserAgent"),
                        TimeConnected = (long)client.Element("Connected"),
                        UserId = (string)client.Element("ID")
                    };

                    liveState.Devices.Add(device);
                }

                liveState.DeviceCount = liveState.Devices.Count;

                liveStates.Add(liveState);
            }

            return liveStates;
        }
    }
}
