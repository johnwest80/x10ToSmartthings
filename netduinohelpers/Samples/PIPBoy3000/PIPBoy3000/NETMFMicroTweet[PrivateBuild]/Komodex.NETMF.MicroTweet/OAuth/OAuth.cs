using System;
using Microsoft.SPOT;
using VikingErik.NetMF.MicroLinq;
using System.Net;
using System.Text;
using System.IO;
using ElzeKool;
using System.Collections;
using Komodex.NETMF.MicroTweet.HTTP;

namespace Komodex.NETMF.MicroTweet
{
    // MicroTweet
    // Matt Isenhower, Komodex Systems LLC
    // http://microtweet.codeplex.com

    public partial class OAuth
    {
        #region Fields

        protected readonly string consumerKey;
        protected readonly string consumerSecret;

        #endregion

        public OAuth(string consumerKey, string consumerSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
        }

        #region Public Methods

        /// <summary>
        /// Creates an OAuth HTTP request.
        /// </summary>
        /// <param name="url">The request URL (without query string parameters)</param>
        /// <param name="method">Must be equal to "GET" or "POST"</param>
        /// <param name="consumerKey">The OAuth Consumer Key</param>
        /// <param name="consumerSecret">The OAuth Consumer Secret</param>
        /// <param name="parameters">A NearlyGenericArrayList of QueryParameter objects, or null if no parameters are needed</param>
        /// <param name="token">The OAuth access token</param>
        /// <param name="tokenSecret">The OAuth token secret</param>
        /// <returns>An HttpWebResponse object</returns>
        public HttpRequest GetOAuthRequest(string url, string method, ArrayList parameters = null, string token = null, string tokenSecret = null)
        {
            SendDebugMessage("Creating OAuth HTTP Request for: " + url);

            ArrayList oauthParams = GenerateOAuthParameters(token);

            // Get a list of all parameters for the OAuth signature
            ArrayList allParams = new ArrayList();
            foreach (var item in oauthParams)
                allParams.Add(item);
            if (parameters != null)
            {
                foreach (var item in parameters)
                    allParams.Add(item);
            }

            // Get the signature
            SendDebugMessage("Signing request...");
            string oauthSignature = GenerateOAuthSignature(url, method, allParams, tokenSecret);
            SendDebugMessage("Done.");

            // Add the signature to the OAuth parameters
            oauthParams.Add(new QueryParameter("oauth_signature", oauthSignature));

            // Generate the OAuth HTTP Authorization header string
            string httpAuthorizationHeaderString = "OAuth ";
            foreach (var item in oauthParams)
            {
                QueryParameter p = (QueryParameter)item;

                httpAuthorizationHeaderString += p.Key + "=\"" + HTTPUtility.UrlEncode(p.Value) + "\"";
                if (oauthParams.IndexOf(item) < oauthParams.Count - 1)
                    httpAuthorizationHeaderString += ", ";
            }

            // Add the parameters to the query string if this is a GET request
            if (method == "GET" && parameters != null)
                url += "?" + HTTPUtility.ParameterListToQueryString(parameters);

            // Create the HttpRequest
            HttpRequest httpRequest = new HttpRequest(url);
            httpRequest.Method = method;
            httpRequest.Headers["Authorization"] = httpAuthorizationHeaderString;

            SendDebugMessage("Submitting request...");

            // Send POST content
            if (method == "POST")
            {
                httpRequest.ContentType = "application/x-www-form-urlencoded";
                if (parameters != null)
                     httpRequest.PostContent = HTTPUtility.ParameterListToQueryString(parameters);
            }

            return httpRequest;
        }

        #endregion

        #region "Generate" Methods

        private ArrayList GenerateOAuthParameters(string token)
        {
            ArrayList result = new ArrayList();

            result.Add(new QueryParameter("oauth_consumer_key", consumerKey));
            result.Add(new QueryParameter("oauth_nonce", GenerateNonce()));
            result.Add(new QueryParameter("oauth_signature_method", "HMAC-SHA1"));
            result.Add(new QueryParameter("oauth_timestamp", DateTime.Now.GetTimestamp().ToString()));
            result.Add(new QueryParameter("oauth_version", "1.0"));
            result.Add(new QueryParameter("oauth_token", token));

            return result;
        }

        private static string GenerateNonce()
        {
            const string alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            Random random = new Random();

            char[] nonceChars = new char[16];
            for (int i = 0; i < nonceChars.Length; i++)
            {
                char r = alphanumeric[random.Next(alphanumeric.Length)];
                nonceChars[i] = r;
            }

            return new string(nonceChars);
        }

        private string GenerateOAuthSignature(string url, string method, ArrayList allParameters, string tokenSecret)
        {
            // The parameters must be sorted by key name before generating the signature hash
            //allParameters = (ArrayList)allParameters.OrderBy(s => s);
            //allParameters.OrderBy(s=>s).
            object[] sortedParameters = (object[])allParameters.OrderBy(s => s, (a, b) => ((IComparable)a).CompareTo(b));

            // Get the parameters in the form of a query string
            string normalizedParameters = HTTPUtility.ParameterListToQueryString(sortedParameters);

            // Concatenate the HTTP method, URL, and parameter string
            string signatureBase = method + '&' + HTTPUtility.UrlEncode(url) + '&' + HTTPUtility.UrlEncode(normalizedParameters);

            // Get a byte array of the signatureBase
            byte[] data = Encoding.UTF8.GetBytes(signatureBase);

            // Get the key
            string key = consumerSecret + '&' + tokenSecret;
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // Compute the HMAC-SHA1 hash
            byte[] hashBytes = SHA.computeHMAC_SHA1(keyBytes, data);

            // The signature is a Base64 encoded version of the hash
            return HTTPUtility.ConvertToBase64String(hashBytes);
        }

        #endregion

        #region Debug Messages

        public event DebugMessageEventHandler DebugMessage;

        protected void SendDebugMessage(string s)
        {
            if (DebugMessage != null)
                DebugMessage(this, new DebugMessageEventArgs(s));
        }

        #endregion
    }
}
