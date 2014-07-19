using System;
using Microsoft.SPOT;

namespace Komodex.NETMF.MicroTweet
{
    public delegate void DebugMessageEventHandler(object sender, DebugMessageEventArgs e);

    public class DebugMessageEventArgs : EventArgs
    {
        internal DebugMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; protected set; }
    }
}
