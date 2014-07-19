using System;

namespace x10tosmartthings
{
    public class ST
    {
        public static string GetPayloadString(string str)
        {
            if (str.IndexOf("T00000000:RX") == -1)
                return null;

            string payload = "";
            try
            {
                string[] payloadHex = str.Split('[')[1].Split(']')[0].Split(' ');
                foreach (string hexStr in payloadHex)
                {
                    payload += (char) Convert.ToInt32(hexStr, 16);
                }
            }
            catch (Exception ex)
            {
                payload += " -> Error: " + ex;
            }

            return payload;
        }
    }
}