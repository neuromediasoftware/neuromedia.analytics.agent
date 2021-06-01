using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace NeuroMedia.Analytics.Agent.Filters
{
    [Export(typeof(ILiveStateFilter))]
    public class LiveStateIpHashingFilter : ILiveStateFilter
    {
        public void OnLoaded(IEnumerable<LiveState> liveStates)
        {
            
        }

        public void OnParsed(IEnumerable<LiveState> liveStates)
        {
            foreach (var liveState in liveStates)
            {
                if (liveState.Devices == null || liveState.Devices.Count == 0)
                    continue;

                foreach (var connectedDevice in liveState.Devices) // Hash IP with salt PP. Might add current date in the future
                    connectedDevice.IpAddress = BCrypt.Net.BCrypt.HashPassword(connectedDevice.IpAddress + liveState.PublishingPoint);
            }
        }
    }
}
