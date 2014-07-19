using System;
using Microsoft.SPOT;
using System.Net;
using System.IO;

namespace Komodex.NETMF.MicroTweet
{
    // MicroTweet
    // Matt Isenhower, Komodex Systems LLC
    // http://microtweet.codeplex.com

    public partial class TwitterClient
    {
        #region Fields

        protected readonly string consumerKey;
        protected readonly string consumerSecret;
        protected readonly OAuth OAuth;

        #endregion

        public TwitterClient(string consumerKey, string consumerSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            OAuth = new OAuth(consumerKey, consumerSecret);
            OAuth.DebugMessage += new DebugMessageEventHandler(OAuth_DebugMessage);
        }

        public TwitterClient(string consumerKey, string consumerSecret, string userToken, string userTokenSecret)
            : this(consumerKey, consumerSecret)
        {
            UserToken = userToken;
            UserTokenSecret = userTokenSecret;
        }

        #region Properties

        public string UserToken { get; set; }
        public string UserTokenSecret { get; set; }

        #endregion

        #region Debug Messages

        public event DebugMessageEventHandler DebugMessage;

        protected void SendDebugMessage(string s)
        {
            if (DebugMessage != null)
                DebugMessage(this, new DebugMessageEventArgs(s));
        }

        void OAuth_DebugMessage(object sender, DebugMessageEventArgs e)
        {
            SendDebugMessage(e.Message);
        }

        #endregion
    }
}
