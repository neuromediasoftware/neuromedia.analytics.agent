using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NeuroMedia.Analytics.Agent.Parsing
{
    public interface ILiveStateParser
    {
        //string Name { get; }
        IList<LiveState> GetLiveState(Uri uri, string apiKey);
    }
}
