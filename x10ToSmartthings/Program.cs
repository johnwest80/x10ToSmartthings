using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace x10tosmartthings
{
    public class Program
    {
        private static readonly X10 x10 = new X10(SerialPorts.COM1);
        private static readonly SerialPort smartthings = new SerialPort(SerialPorts.COM2, 2400, Parity.None, 8, StopBits.One);
        private static InterruptPort button;
        private static int GlitchDelay = 500;  //only for the button
        private static DateTime TimerStart;//only for the button

        public static void Main()
        {
            //just used for the internal button
            //the glitch delay is for the button as well
            button = new InterruptPort(Pins.ONBOARD_SW1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            button.Resistor = Port.ResistorMode.PullUp;
            button.OnInterrupt += button_OnInterrupt;
            X10.ActionEvent += CaptureX10Action;

            smartthings.Open();

            // add an event-handler for handling incoming data
            smartthings.DataReceived += smartthings_DataReceived;

            //sound my chime device on my network
            x10.QueueDeviceCommand(new X10.Command("I16") {Action = "On"});

            Logging.Init();

            int x = 1;
            do
            {
                if (x == 200) //total to 1000, which is one second
                {
                    x = 0;
                    Logging.ToggleStatusDot();
                }
                Thread.Sleep(5);
                try
                {
                    x10.Loop();
                }
                catch (Exception ex)
                {
                    Logging.DebugPrint("E x10 loop: " + ex);
                }
                x++;
            } while (1 == 1);
        }

        private static void CaptureX10Action(string device, string action)
        {
            sendStringToCloud(device + "-" + action);
            Logging.DebugPrint("->ST: " + device + "-" + action);
        }

        private static void sendStringToCloud(string str)
        {
            //Logging.DebugPrint("Writing to cloud: " + str);

            string encoded = "raw 0x0 { 00 00 0A 0A ";

            byte[] encodeBytes = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < encodeBytes.Length; i++)
            {
                encoded += encodeBytes[i].ToString("x2") + " ";
            }

            encoded += "}\nsend 0x0 1 1\n";

            // send the bytes over the serial-port
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(encoded);
            smartthings.Write(utf8Bytes, 0, utf8Bytes.Length);
        }


        private static void smartthings_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Logging.DebugPrint("--- ST: " + DateTime.Now + "---");

            string rawstr = "";
            // as long as there is data waiting to be read
            while (smartthings.BytesToRead > 0)
            {
                var bytes = new Byte[smartthings.BytesToRead];
                smartthings.Read(bytes, 0, bytes.Length);
                char[] line = Encoding.UTF8.GetChars(bytes, 0, bytes.Length);
                rawstr += new String(line);
                Thread.Sleep(20);
            }

            //Logging.DebugPrint("Received raw: " + rawstr);

            string[] strs = rawstr.Split('\n');
            foreach (string str in strs)
            {
                if (str != null && str != "\n")
                {
                    string payload = ST.GetPayloadString(str);

                    if (payload != null && payload.IndexOf("X10-") == 1 && payload.Split('-').Length == 3)
                    {
                        Logging.DebugPrint("ST->: " + payload);
                        string device = payload.Split('-')[1];
                        string action = payload.Split('-')[2];

                        if (device != "null" && action != "null")
                            x10.QueueDeviceCommand(new X10.Command(device) {Action = action});
                    }
                }
            }
        }

        /// <summary>
        /// This was hardcoded to my x10... I16 is my chime, which sounds every time this netduino app starts up
        /// and when i push the onboard button
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <param name="time"></param>
        private static void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            DateTime EndTime = DateTime.Now;
            if (EndTime.Subtract(TimerStart).Ticks/10000 > GlitchDelay)
            {
                //sendStringToCloud("helm of success " + DateTime.Now.ToString());
                x10.QueueDeviceCommand(new X10.Command("I16") {Action = "On"});
                TimerStart = DateTime.Now;
            }
        }
    }
}