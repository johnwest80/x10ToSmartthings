using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Komodex.NETMF.MicroTweet;
using netduino.helpers.Helpers;

namespace PIPBoy3000 {
    public class PipBoy3000 {
        private static readonly GeigerCounterStats Stats = new GeigerCounterStats(StatsChangeHandler);
        private static readonly SerialPort Port = new SerialPort(Serial.COM1, 9600, Parity.None, 8, StopBits.One);
        private static readonly System.Text.UTF8Encoding Encoding = new System.Text.UTF8Encoding();
        private static readonly ArrayList Text = new ArrayList();
        private static TwitterClient _twitter;
        private static int _cpm10Seconds;
        private static int _cpmAverage;
        private static int _cpmPreviousAverage = 0;
        private const int TwitterUpdateRateMinutes = 10;
        private static int _twitterCountDown = TwitterUpdateRateMinutes;
        private static bool _updateLCD = true;
        private static bool _updateTwitter;
        private static string _ntpServers;
        private static string _latitude;
        private static string _longitude;

        public static void Main() {

            //Debug.EnableGCMessages(true);

            InitializeLCDScreen();
            InitializeResources();
            
            HelloWasteland();

            var geigerCounter = new GeigerCounter(Pins.GPIO_PIN_D7, Stats);
            geigerCounter.Start();

            while (true) {
                Thread.Sleep(1000);

                Trace.Print("Rnd=" + geigerCounter.GetRandomNumber().ToString());

                if(_updateLCD){
                    _updateLCD = false;
                    WriteLCD(LcdCommandClearScreen + _cpm10Seconds);
                }

                if(_updateTwitter) {
                    _updateTwitter = false;

                    if (_cpmPreviousAverage != _cpmAverage) {
                        _cpmPreviousAverage = _cpmAverage;

                        Text.Clear();
                        Text.Add(GetUtcTimestamp());
                        Text.Add(",Lat=" + _latitude);
                        Text.Add(",Long=" + _longitude);
                        Text.Add(",Rnd=" + geigerCounter.GetRandomNumber());
                        Text.Add(",AverageCPM(Hour)=" + _cpmAverage);

                        TwitterStatusUpdate(Text);
                    }
                }
            }
        }

        private static void HelloWasteland() {
            SetInternalClock();

            Text.Clear();
            Text.Add(GetUtcTimestamp());
            Text.Add(",Status=PIPBoy3000 Booted");

            TwitterStatusUpdate(Text);
        }

        private static string GetUtcTimestamp() {
            return "UTCTime=" + DateTime.Now;
        }

        private static void SetInternalClock() {
            try {
                var servers = _ntpServers.Split(',');
                Trace.Print("Begin SetSystemTimeUTC");
                NTPTime.SetSystemTimeUTC(NTPTime.GetNTPTime(servers));
                Trace.Print("End SetSystemTimeUTC");
            } catch {
                // Failed to set system time due to all NTP servers failing.
                // To clear a possible network stack issue, reboot the device!
                PowerState.RebootDevice(false);
            }
        }

        public static void TwitterStatusUpdate(ArrayList txt) {
            try {
                var status = "";
                foreach (string str in txt) {
                    status += str;
                }
                SetInternalClock();
                _twitter.SendTweet(status);
            } catch (Exception e) {
                // Nothing to do if this fails. Just don't crash!
            }
        }
        /// <summary>
        /// Configures the application from the 'resources.txt' file place on the microSD card.
        /// See the 'SD Card Resources' folder for a sample to start with.
        /// </summary>
        private static void InitializeResources() {
            var resourceLoader = new SDResourceLoader();

            resourceLoader.Load();

            _ntpServers = (string)resourceLoader.Strings["ntpServers"];
            _latitude = (string)resourceLoader.Strings["latitude"];
            _longitude = (string)resourceLoader.Strings["longitude"];

            _twitter = new TwitterClient(
                            (string)resourceLoader.Strings["consumerKey"],
                            (string)resourceLoader.Strings["consumerSecret"],
                            (string)resourceLoader.Strings["accessToken"],
                            (string)resourceLoader.Strings["accessTokenSecret"]);

            resourceLoader.Dispose();

            Debug.GC(true);
        }

        private const string LcdCommandSetGeometry = @"?>4";
        private const string LcdCommandClearScreen = @"?f";

        private static void InitializeLCDScreen() {
            Port.Open();
            Thread.Sleep(5000);
            WriteLCD(LcdCommandSetGeometry);
            Thread.Sleep(200);
            WriteLCD(LcdCommandClearScreen);
        }

        public static void WriteLCD(string str) {
            Port.Write(Encoding.GetBytes(str), 0, str.Length);
        }

        public static void StatsChangeHandler(StatsChangeEvent changeType, int data, int minuteCounter) {
            switch (changeType) {
                case StatsChangeEvent.ClicksPerTimeSlice:
                    _cpm10Seconds = data;
                    _updateLCD = true;
                    break;
                case StatsChangeEvent.ClicksPerMinute:
                    break;
                case StatsChangeEvent.ClicksPerHourAverage:
                    _cpmAverage = data;
                    _updateLCD = true;
                    _twitterCountDown--;
                    if (_twitterCountDown == 0) {
                        _twitterCountDown = TwitterUpdateRateMinutes;
                        _updateTwitter = true;
                    }
                    break;
            }
        }
    }
}
