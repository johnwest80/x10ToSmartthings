using System;
using Microsoft.SPOT;

namespace Komodex.NETMF.MicroTweet.HTTP
{
    public class HttpUri
    {
        public HttpUri(string uri)
        {
            ParseUri(uri);
        }

        #region Properties

        public string FullUri { get; protected set; }
        public string Hostname { get; protected set; }
        public int Port { get; protected set; }
        public string Path { get; protected set; }

        #endregion

        #region Methods

        protected void ParseUri(string uri)
        {
            if (uri == null || uri == string.Empty)
                throw new ArgumentNullException("uri");
            if (uri.IndexOf("http://") != 0)
                throw new ArgumentException("Only HTTP URIs are supported.");

            FullUri = uri;

            int hostStart = 7;
            int portStart = uri.IndexOf(':', hostStart);
            int pathStart = uri.IndexOf('/', hostStart);
            int hostEnd;

            if (portStart > 0 && portStart < pathStart)
            {
                // A port number was specified
                string portString = uri.Substring(portStart + 1, pathStart - portStart - 1);
                Port = int.Parse(portString);
                hostEnd = portStart;
            }
            else
            {
                // No port number was specified
                Port = 80;
                if (pathStart > 0)
                    hostEnd = pathStart;
                else
                    hostEnd = uri.Length;
            }

            Hostname = uri.Substring(hostStart, hostEnd - hostStart);

            if (pathStart > 0)
                Path = uri.Substring(pathStart);
            else
                Path = string.Empty;
        }

        public override string ToString()
        {
            return FullUri;
        }

        #endregion
    }
}
