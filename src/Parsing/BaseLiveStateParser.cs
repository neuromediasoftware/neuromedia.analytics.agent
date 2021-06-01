using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NeuroMedia.Analytics.Agent.Parsing
{
    public abstract class BaseLiveStateParser
    {
        protected WebClient GetClientFromUri(Uri uri)
        {
            // Creates the web client to do the 2 requests
            WebClient webClient = new WebClient(); //TODO replace by HttpClient to include timeout

            if (!string.IsNullOrWhiteSpace(uri.UserInfo))
            {
                var credentials = uri.UserInfo.Split(':');
                webClient.Credentials = new NetworkCredential(credentials[0], credentials[1]);
            }

            return webClient;
        }
    }
}
