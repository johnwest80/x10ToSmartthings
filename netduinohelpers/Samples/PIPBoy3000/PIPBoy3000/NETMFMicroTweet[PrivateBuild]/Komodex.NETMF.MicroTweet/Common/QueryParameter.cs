using System;
using Microsoft.SPOT;

namespace Komodex.NETMF.MicroTweet
{
    class QueryParameter : IComparable
    {
        public QueryParameter(string key, string value)
        {
            Key = key;
            Value = value;
        }

        #region Properties

        public string Key { get; protected set; }
        public string Value { get; protected set; }

        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            QueryParameter compared = (QueryParameter)obj;

            if (this.Key == compared.Key)
                return this.Value.CompareTo(compared.Value);
            return this.Key.CompareTo(compared.Key);
        }

        #endregion
    }
}
