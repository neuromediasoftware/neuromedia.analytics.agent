using MaxMind.Db;
using MaxMind.GeoIP2;
using NeuroMedia.Analytics.LiveWatch.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Net;
using System.Text;

namespace NeuroMedia.Analytics.Agent.Filters
{
    [Export(typeof(ILiveStateFilter))]
    public class LiveStateMaxMindLocationDetectionFilter : ILiveStateFilter
    {
        public void OnLoaded(IEnumerable<LiveState> liveStates)
        {
            
        }

        public void OnParsed(IEnumerable<LiveState> liveStates)
        {
            try
            {
                using (var reader = new DatabaseReader("GeoIP2-City.mmdb"))
                {
                    foreach (var liveState in liveStates)
                    {
                        if (liveState.Devices == null || liveState.Devices.Count == 0)
                            continue;

                        foreach (var connectedDevice in liveState.Devices)
                        {
                            try
                            {
                                var geoData = reader.City(connectedDevice.IpAddress);

                                if (geoData == null)
                                    continue;

                                connectedDevice.Location = new DeviceGeoLocation();

                                if (geoData.City != null)
                                    connectedDevice.Location.City = geoData.City.Name;

                                if (geoData.Country?.IsoCode != null)
                                    connectedDevice.Location.CountryCode = geoData.Country.IsoCode.ToLower();

                                if (geoData.Country?.Name != null)
                                    connectedDevice.Location.CountryName = geoData.Country.Name.ToLower();

                                if (geoData.Continent?.Name != null)
                                    connectedDevice.Location.ContinentName = geoData.Continent.Name.ToLower();

                                if (geoData.Location.Longitude != null)
                                    connectedDevice.Location.Longitude = (decimal)geoData.Location.Longitude;

                                if (geoData.Location.Latitude != null)
                                    connectedDevice.Location.Latitude = (decimal)geoData.Location.Latitude;

                                if (geoData.Subdivisions.Count > 0)
                                    connectedDevice.Location.Region = geoData.Subdivisions[0].Name;

                                if (geoData.Subdivisions.Count > 1)
                                    connectedDevice.Location.SubRegion = geoData.Subdivisions[1].Name;
                            }
                            catch (Exception ex)
                            {
                                // Failed
                            }
                        }
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //throw new Exception
            }
        }
    }
}
