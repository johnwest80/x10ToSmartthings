using System;
using System.Collections;
using System.IO.Ports;

namespace x10tosmartthings
{
    public class X10
    {
        public delegate void ActionDelegate(string device, string action);

        public static SerialPort Serial;

        public static ArrayList IncomingBytes = new ArrayList();

        public X10(string com)
        {
            Queue = new ArrayList();
            Serial = new SerialPort(com, 4800, Parity.None, 8, StopBits.One);
            Serial.ReadTimeout = 3000;
            Serial.Open();
        }

        public static string LastIncomingAddress { get; set; }
        public IList Queue { get; set; }
        public static event ActionDelegate ActionEvent;

        public void QueueDeviceCommand(Command command)
        {
            Queue.Add(command);
            Logging.DebugPrint("->x10: " + command);
        }


        public void Loop()
        {
            if (Serial.BytesToRead > 0)
            {
                int b = Serial.ReadByte();
                if (b == 90)
                    ProcessIncoming();
            }
            if (Queue.Count > 0)
                TalkToDevice((Command) Queue[0]);
        }

        private void TalkToDevice(Command command)
        {
            Logging.DebugPrint("x10!: " + command);
            try
            {
                Serial.Flush();
                //send address
                var bytes = new byte[2];
                bytes[0] = 0x04;
                bytes[1] = (byte) (X10Helper.GetHouseCode(command.HouseCode) | (int) (X10Helper.GetDeviceCode(int.Parse(command.DeviceCode))));
                //Logging.DebugPrint("Talk 2 bytes: " + bytes[0] + "/" + bytes[1]);
                Serial.Write(bytes, 0, bytes.Length);
                //verify checksum
                int checksum = Serial.ReadByte();
                //Logging.DebugPrint("Talk checksum: " + checksum);

                if (checksum == X10Helper.Checksum(bytes, 2))
                {
                    //Logging.DebugPrint("Talk checksum matched");
                    Serial.WriteByte(0);
                }
                //Logging.DebugPrint("Talk get confirmation");
                //ok to send
                int talkConf = Serial.ReadByte();
                //Logging.DebugPrint("Talk get confirmation = " + talkConf);
                if (talkConf == 0x55)
                {
                    var actionbytes = new byte[2];
                    actionbytes[0] = 0x06;
                    actionbytes[1] = (byte) (X10Helper.GetHouseCode(command.HouseCode) | (int) (X10Helper.GetFunctionFromName(command.Action)));
                    //Logging.DebugPrint("Talk action 2 bytes: " + actionbytes[0] + "/" + actionbytes[1]);
                    Serial.Write(actionbytes, 0, actionbytes.Length);

                    int actionchecksum = Serial.ReadByte();
                    //Logging.DebugPrint("Talk action checksum: " + actionchecksum);
                    if (actionchecksum == X10Helper.Checksum(actionbytes, 2))
                    {
                        //Logging.DebugPrint("Talk action checksum matched");
                        Serial.WriteByte(0);
                    }
                    //Logging.DebugPrint("Talk action get confirmation");

                    int interfaceReady = Serial.ReadByte();
                    //Logging.DebugPrint("Talk action get confirmation = " + interfaceReady);
                    if (interfaceReady != 0x55)
                    {
                        if (interfaceReady == 90)
                        {
                            Logging.DebugPrint("Diverted from getting conf to processing incoming");
                            ProcessIncoming();
                        }
                        else
                            throw new Exception("Talk action conf error - received byte " + interfaceReady);
                    }
                    else
                    {
                        Queue.RemoveAt(0);
                    }
                }
                else
                {
                    if (talkConf == 90)
                    {
                        Logging.DebugPrint("Diverted from getting talk conf to processing incoming");
                        ProcessIncoming();
                    }
                    else
                        throw new Exception("Talk conf error - received byte " + talkConf);
                }
            }
            catch (Exception ex)
            {
                Logging.DebugPrint("Talk to device error: " + ex);
                Serial.DiscardInBuffer();
                if (command.Retries++ > 5)
                {
                    Logging.DebugPrint("Max retries reached");
                    Queue.RemoveAt(0);
                }
            }
        }

        private static void ProcessIncoming()
        {
            Serial.Flush();
            //send acknowledgement
            Serial.WriteByte(0xc3);

            //get total number of bytes in buffer
            var numBytes = Serial.ReadByte();
            //Logging.DebugPrint("numbytes set to " + numBytes);

            //first byte is function/address mask
            var functionBitFlag = Serial.ReadByte();
            //Logging.DebugPrint("Raw function/address val is " + functionBitFlag);
            numBytes--;

            var curByte = 0; //current data byte after num bytes and mask being looked at in the buffer that was sent

            //gotta have some data to be able to do anything
            while (numBytes > 0 && numBytes <= 8) // can't have more than 8 bytes
            {
                //determine whether it's a function or address coming next
                var function = (functionBitFlag & (int) Math.Pow(2, curByte)) == (int) Math.Pow(2, curByte);
                var address = !function;
                //Logging.DebugPrint("Curbyte " + curByte + " function/address: " + function + "/" + address);

                //get first data byte in buffer
                int firstByte = Serial.ReadByte();

                numBytes--;
                curByte++;

                if (address)
                {
                    var housecode = (X10Helper.HouseCode) (firstByte & (128 + 64 + 32 + 16));
                    int devicecode = X10Helper.GetDeviceNumber((X10Helper.DeviceCode) (firstByte & (8 + 4 + 2 + 1)));
                    //we're not handling situations where multiple devices are acted on at the same time
                    LastIncomingAddress = X10Helper.GetDeviceLetter(housecode) + devicecode;
                    //Logging.DebugPrint("LastIncomingAddress set to " + LastIncomingAddress);
                }
                if (function && LastIncomingAddress != null)
                {
                    var housecode = (X10Helper.HouseCode) (firstByte & (128 + 64 + 32 + 16));
                    string action = X10Helper.GetFunctionName((X10Helper.Function) (firstByte & (8 + 4 + 2 + 1)));

                    if (ActionEvent != null)
                    {
                        ActionEvent(LastIncomingAddress, action);
                        //Logging.DebugPrint("Threw action event " + LastIncomingAddress + "/" + action);
                    }

                    //we're not handling cases where more than one function is done on the same device
                    LastIncomingAddress = null;
                }
            }
        }

        public class Command
        {
            public Command(string device)
            {
                HouseCode = device.Substring(0, 1);
                DeviceCode = device.Substring(1);
                Retries = 0;
            }

            public string DeviceCode { get; set; }
            public string HouseCode { get; set; }
            public string Action { get; set; }
            public int Retries { get; set; }

            public override string ToString()
            {
                return "" + HouseCode + DeviceCode + "-Ret: " + Retries;
            }
        }
    }
}