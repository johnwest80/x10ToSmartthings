using System;
using Microsoft.SPOT;

namespace Komodex.NETMF.MicroTweet
{
    static class Utility
    {
        #region DateTime Extension Methods

        /// <summary>
        /// Returns the number of seconds since January 1, 1970
        /// </summary>
        public static long GetTimestamp(this DateTime dt)
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            TimeSpan timeSpan = (DateTime.Now - epoch);
            return timeSpan.Ticks / 10000000;
        }

        #endregion

        #region String Extension Methods

        public static string Replace(this string s, char oldChar, char newChar)
        {
            char[] newChars = new char[s.Length];
            for (int i = 0; i < s.Length; i++)
                newChars[i] = (s[i] == oldChar) ? newChar : s[i];
            return new string(newChars);
        }

        #endregion
    }
}

// ExtensionAttribute must be added manually
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}
