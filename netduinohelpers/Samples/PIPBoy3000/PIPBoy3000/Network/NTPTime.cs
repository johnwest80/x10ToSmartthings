using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Hardware;

namespace PIPBoy3000 {
    public class NTPTime {
        public static void SetSystemTimeUTC(DateTime utcTime) {
            Utility.SetLocalTime(utcTime);
        }

        /// <summary>
        /// Retrieve a timestamp from an NTP server
        /// http://tools.ietf.org/html/rfc2030 
        /// </summary>
        /// <param name="servers">NTP server domain name</param>
        /// <returns>A UTC timestamp</returns>
        public static DateTime GetNTPTime(string[] servers) {
            var ran = new Random(DateTime.Now.Millisecond);

            byte[] ntpData = new byte[48];

            for (int i = 0; i < servers.Length; i++) {
                try {
                    Trace.Print("NTP Begin DNS");
                    var ep = new IPEndPoint(Dns.GetHostEntry(servers[ran.Next(servers.Length)]).AddressList[0], 123);
                    Trace.Print("NTP End DNS");

                    Array.Clear(ntpData, 0, ntpData.Length);
                    ntpData[0] = 0x1B;
                    int receivedBytes = 0;
                    int sentBytes = 0;

                    using (var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                        s.SendTimeout = 1000;
                        s.ReceiveTimeout = 5000;

                        Trace.Print("NTP Socket Connect");
                        s.Connect(ep);

                        Trace.Print("NTP Socket Send");
                        sentBytes = s.Send(ntpData);

                        if (sentBytes != 0) {
                            Trace.Print("NTP Socket Receive");
                            receivedBytes = s.Receive(ntpData);
                        }

                        Trace.Print("NTP Socket Close");
                        s.Close();
                    }

                    if (receivedBytes > 0) {
                        byte offsetTransmitTime = 40;
                        ulong intpart = 0;
                        ulong fractpart = 0;

                        for (int x = 0; x <= 3; x++) {
                            intpart = 256 * intpart + ntpData[offsetTransmitTime + x];
                        }

                        for (int x = 4; x <= 7; x++) {
                            fractpart = 256 * fractpart + ntpData[offsetTransmitTime + x];
                        }

                        ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);

                        if (ntpData[47] != 0) {
                            TimeSpan timeSpan = TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
                            DateTime networkDateTime = new DateTime(1900, 1, 1);
                            networkDateTime += timeSpan;
                            return networkDateTime;
                        }
                    }
                } catch (Exception) {
                    // Just try the next server...
                    Trace.Print("NTP failure. Next NTP Server...");
                }
            }
            // All servers were tried and failed!
            throw new ApplicationException("servers");
        }
    }
}
