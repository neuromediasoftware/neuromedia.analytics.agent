using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeuroMedia.Analytics.Agent.Filters
{
    public interface ILiveStateFilter
    {
        void OnLoaded(IEnumerable<LiveState> liveStates);
        void OnParsed(IEnumerable<LiveState> liveStates);
    }
}
