using System;
using Microsoft.SPOT;
using System.Collections;
using VikingErik.NetMF.MicroLinq;

namespace Komodex.NETMF.MicroTweet.HTTP
{
    public class HeaderCollection
    {
        #region Internal Name/Value Pair ArrayList

        protected ArrayList headerNameValuePairs = new ArrayList();

        protected HeaderValuePair HeaderValuePairForHeader(string header)
        {
            return headerNameValuePairs.FirstOrDefault(hvp => ((HeaderValuePair)hvp).Key == header) as HeaderValuePair;
        }

        #endregion

        #region Indexer

        public string this[string name]
        {
            get
            {
                // Attempt to locate the name/value pair in the internal ArrayList
                HeaderValuePair valuePair = HeaderValuePairForHeader(name);

                // If the name/value pair was found, return the value
                if (valuePair != null)
                    return valuePair.Value;
                return null;
            }
            set { Set(name, value); }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            string result = string.Empty;
            foreach (HeaderValuePair headerPair in headerNameValuePairs)
                result += headerPair.Key + ": " + headerPair.Value;
            return result;
        }

        public void Set(string name, string value)
        {
            // If the value is null or empty, remove the item from the list
            if (value == null || value == string.Empty)
            {
                Remove(name);
                return;
            }

            // Attempt to locate the name/value pair in the internal ArrayList
            HeaderValuePair valuePair = HeaderValuePairForHeader(name);

            // If the key already exists, update its value
            if (valuePair != null)
                valuePair.Value = value;

            // Otherwise, add a new name/value pair
            else
                headerNameValuePairs.Add(new HeaderValuePair(name, value));
        }

        public void Remove(string name)
        {
            // Attempt to locate the name/value pair in the internal ArrayList
            HeaderValuePair valuePair = HeaderValuePairForHeader(name);

            // If the name/value pair exists, remove it from the ArrayList
            if (valuePair != null)
                headerNameValuePairs.Remove(valuePair);
        }

        #endregion

    }
}
