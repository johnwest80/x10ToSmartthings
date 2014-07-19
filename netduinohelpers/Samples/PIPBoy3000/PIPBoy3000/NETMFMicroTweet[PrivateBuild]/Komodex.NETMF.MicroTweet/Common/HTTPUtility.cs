using System;
using Microsoft.SPOT;
using System.Collections;

namespace Komodex.NETMF.MicroTweet
{
    public static class HTTPUtility
    {
        #region URL Encoding

        public static string UrlEncode(string s)
        {
            if (s == null)
                return null;

            string result = string.Empty;

            for (int i = 0; i < s.Length; i++)
            {
                if (ShouldEncodeChar(s[i]))
                    result += '%' + ByteToHex((byte)s[i]);
                else
                    result += s[i];
            }

            return result;
        }

        private static bool ShouldEncodeChar(char c)
        {
            // Safe characters defined by RFC3986:
            // http://oauth.net/core/1.0/#encoding_parameters

            if (c >= '0' && c <= '9')
                return false;
            if (c >= 'A' && c <= 'Z')
                return false;
            if (c >= 'a' && c <= 'z')
                return false;
            switch (c)
            {
                case '-':
                case '.':
                case '_':
                case '~':
                    return false;
            }

            // All other characters should be encoded
            return true;
        }

        public static string ByteToHex(byte b)
        {
            const string hex = "0123456789ABCDEF";
            int lowNibble = b & 0x0F;
            int highNibble = (b & 0xF0) >> 4;
            string s = new string(new char[] { hex[highNibble], hex[lowNibble] });
            return s;
        }

        #endregion

        #region Query Strings

        public static string ParameterListToQueryString(IList parameters)
        {
            if (parameters == null)
                return null;

            string result = string.Empty;

            for (int i = 0; i < parameters.Count; i++)
            {
                QueryParameter p = (QueryParameter)parameters[i];
                result += UrlEncode(p.Key) + '=' + UrlEncode(p.Value);
                if (i < parameters.Count - 1)
                    result += '&';
            }

            return result;
        }

        #endregion

        #region Base64

        // Adapted from example at: http://en.wikibooks.org/wiki/Algorithm_Implementation/Miscellaneous/Base64#Java
        public static string ConvertToBase64String(byte[] inArray)
        {
            // Base 64 character set
            const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

            // Result and padding strings
            string r = string.Empty;
            string p = string.Empty;

            // Zero pad the array if necessary
            int c = inArray.Length % 3;
            if (c > 0)
            {
                int addedChars = 3 - c;
                p = new string('=', addedChars);
                byte[] newArray = new byte[inArray.Length + addedChars];
                inArray.CopyTo(newArray, 0);
                inArray = newArray;
            }

            // Convert the input array
            for (int i = 0; i < inArray.Length; i += 3)
            {
                // Add a newline character if necessary
                if (i > 0 && (i / 3 * 4) % 76 == 0)
                    r += "\r\n";

                // Three bytes become one 24-bit number
                int n = (inArray[i] << 16) + (inArray[i + 1] << 8) + (inArray[i + 2]);

                // 24-bit number gets split into four 6-bit numbers
                int n1 = (n >> 18) & 63;
                int n2 = (n >> 12) & 63;
                int n3 = (n >> 6) & 63;
                int n4 = n & 63;

                // Use the four 6-bit numbers as indices for the base64 character list
                r = string.Concat(r, base64Chars[n1], base64Chars[n2], base64Chars[n3], base64Chars[n4]);
            }

            return r.Substring(0, r.Length - p.Length) + p;
        }

        #endregion
    }
}
