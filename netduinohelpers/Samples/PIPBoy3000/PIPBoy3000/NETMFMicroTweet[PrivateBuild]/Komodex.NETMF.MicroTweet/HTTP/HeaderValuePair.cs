using System;
using Microsoft.SPOT;

namespace Komodex.NETMF.MicroTweet.HTTP
{
    public class HeaderValuePair
    {
        public HeaderValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        #region Properties

        public string Key { get; set; }
        public string Value { get; set; }

        #endregion
    }
}
