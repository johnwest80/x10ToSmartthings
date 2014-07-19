using System;
using Microsoft.SPOT;
using System.Net;
using System.IO;
using System.Collections;
using Komodex.NETMF.MicroTweet.HTTP;

namespace Komodex.NETMF.MicroTweet
{
    // MicroTweet
    // Matt Isenhower, Komodex Systems LLC
    // http://microtweet.codeplex.com

    public partial class TwitterClient
    {
        #region Sending a Tweet

        public bool SendTweet(string message)
        {
            const string sendTweetUrl = "http://api.twitter.com/1/statuses/update.xml";

            if (UserToken == null || UserTokenSecret == null)
            {
                SendDebugMessage("Error: Please specify the user token information before attempting to send a tweet.");
                return false;
            }

            ArrayList parameters = new ArrayList();
            parameters.Add(new QueryParameter("status", message));
            parameters.Add(new QueryParameter("trim_user", "1"));

            HttpRequest httpRequest = OAuth.GetOAuthRequest(sendTweetUrl, "POST", parameters, UserToken, UserTokenSecret);

            // Fire off the message to twitter without looking at the response to avoid an OutOfMemoryException
            httpRequest.GetResponse(false);
            
            return true;
        }

        #endregion

    }
}
