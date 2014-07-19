using System;
using Microsoft.SPOT;

// Some of this code comes from Craig's Creations at http://www.craigscreations.com/projects.html
namespace x10tosmartthings
{
    public class X10Helper
    {
        [Flags]
        public enum HouseCode
        {
            A = 0x60,
            B = 0xE0,
            C = 0x20,
            D = 0xA0,
            E = 0x10,
            F = 0x90,
            G = 0x50,
            H = 0xD0,
            I = 0x70,
            J = 0xF0,
            K = 0x30,
            L = 0xB0,
            M = 0X00,
            N = 0x80,
            O = 0x40,
            P = 0xC0
        }

        [Flags]
        public enum Function
        {
            AllUnitsOff = 0x00,
            AllLightsOn = 0x01,
            On = 0x02,
            Off = 0x03,
            Dim = 0x04,
            Bright = 0x05,
            AllLightsOff = 0x06,
            ExtendedCode = 0x07,
            HailRequest = 0x08,
            HailAcknowledge = 0x09,
            PresetDim1 = 0x0A,
            PresetDim2 = 0x0B,
            ExtededDataTransfer = 0x0C,
            StatusOn = 0X0D,
            StatusOff = 0x0E,
            StatusRequest = 0x0F
        }
        [Flags]
        public enum DeviceCode //for cm11a
        {
            ONE = 0x06,
            TWO = 0x0E,
            THREE = 0x02,
            FOUR = 0x0A,
            FIVE = 0x01,
            SIX = 0x09,
            SEVEN = 0x05,
            EIGHT = 0x0D,
            NINE = 0x07,
            TEN = 0x0F,
            ELEVEN = 0x03,
            TWELVE = 0x0B,
            THIRTEEN = 0X00,
            FOURTEEN = 0x08,
            FIFTEEN = 0x04,
            SIXTEEN = 0x0C
        }

        [Flags]
        public enum Header
        {
            Address = 0x04,
            ExtendedTransmission = 0x05,
            Function = 0x06
        }

        /// <summary>
        ///     Computes Checksum of transmission to determine if the controller received command
        ///     properly
        /// </summary>
        /// <param name="buffer">Command to send in the form of a byte array.</param>
        /// <param name="count">length of command array</param>
        /// <returns></returns>
        public static int Checksum(byte[] buffer, int count)
        {
            if (1 < count)
            {
                byte iRetVal = 0;
                foreach (byte element in buffer)
                {
                    iRetVal += element;
                }
                return (iRetVal & 0xFF);
            }
            if (1 == count)
            {
                if (0 == buffer[0]) { return (0x55); }
            }
            return (0x00);
        }

        /// <summary>
        ///     Determines the actual Device Code as it appears to the CM11a
        /// </summary>
        /// <param name="deviceCode">Device Code to operate on</param>
        /// <returns>CM11a representation of the device code</returns>
        public static DeviceCode GetDeviceCode(int deviceCode) //for cm11a
        {
            switch (deviceCode)
            {
                case 1:
                    return (DeviceCode.ONE);
                case 2:
                    return (DeviceCode.TWO);
                case 3:
                    return (DeviceCode.THREE);
                case 4:
                    return (DeviceCode.FOUR);
                case 5:
                    return (DeviceCode.FIVE);
                case 6:
                    return (DeviceCode.SIX);
                case 7:
                    return (DeviceCode.SEVEN);
                case 8:
                    return (DeviceCode.EIGHT);
                case 9:
                    return (DeviceCode.NINE);
                case 10:
                    return (DeviceCode.TEN);
                case 11:
                    return (DeviceCode.ELEVEN);
                case 12:
                    return (DeviceCode.TWELVE);
                case 13:
                    return (DeviceCode.THIRTEEN);
                case 14:
                    return (DeviceCode.FOURTEEN);
                case 15:
                    return (DeviceCode.FIFTEEN);
                case 16:
                    return (DeviceCode.SIXTEEN);
            }
            throw (new ApplicationException("Device Code out of range"));
        }

        public static int GetHouseCode(string housecode)
        {
            switch (housecode)
            {
                case "A":
                    return (int) HouseCode.A;
                case "B":
                    return (int) HouseCode.B;
                case "C":
                    return (int) HouseCode.C;
                case "D":
                    return (int) HouseCode.D;
                case "E":
                    return (int) HouseCode.E;
                case "F":
                    return (int) HouseCode.F;
                case "G":
                    return (int) HouseCode.G;
                case "H":
                    return (int) HouseCode.H;
                case "I":
                    return (int) HouseCode.I;
                case "J":
                    return (int) HouseCode.J;
                case "K":
                    return (int) HouseCode.K;
                case "L":
                    return (int) HouseCode.L;
                case "M":
                    return (int) HouseCode.M;
                case "N":
                    return (int) HouseCode.N;
                case "O":
                    return (int) HouseCode.O;
                case "P":
                    return (int) HouseCode.P;
            }
            throw (new ApplicationException("Device Code out of range"));
        }

        public static int GetDeviceNumber(DeviceCode code)
        {
            switch (code)
            {
                case DeviceCode.ONE:
                    return (1);
                case DeviceCode.TWO:
                    return (2);
                case DeviceCode.THREE:
                    return (3);
                case DeviceCode.FOUR:
                    return 4;
                case DeviceCode.FIVE:
                    return (5);
                case DeviceCode.SIX:
                    return (6);
                case DeviceCode.SEVEN:
                    return (7);
                case DeviceCode.EIGHT:
                    return (8);
                case DeviceCode.NINE:
                    return (9);
                case DeviceCode.TEN:
                    return (10);
                case DeviceCode.ELEVEN:
                    return (11);
                case DeviceCode.TWELVE:
                    return (12);
                case DeviceCode.THIRTEEN:
                    return (13);
                case DeviceCode.FOURTEEN:
                    return (14);
                case DeviceCode.FIFTEEN:
                    return (15);
                case DeviceCode.SIXTEEN:
                    return (16);
            }
            throw (new ApplicationException("Device Code out of range"));
        }

        public static string GetDeviceLetter(HouseCode code)
        {
            switch (code)
            {
                case HouseCode.A:
                    return "A";
                case HouseCode.B:
                    return "B";
                case HouseCode.C:
                    return "C";
                case HouseCode.D:
                    return "D";
                case HouseCode.E:
                    return "E";
                case HouseCode.F:
                    return "F";
                case HouseCode.G:
                    return "G";
                case HouseCode.H:
                    return "H";
                case HouseCode.I:
                    return "I";
                case HouseCode.J:
                    return "J";
                case HouseCode.K:
                    return "K";
                case HouseCode.L:
                    return "L";
                case HouseCode.M:
                    return "M";
                case HouseCode.N:
                    return "N";
                case HouseCode.O:
                    return "O";
                case HouseCode.P:
                    return "P";
            }
            throw (new ApplicationException("House Code out of range"));
        }

        public static string GetFunctionName(Function f)
        {
            switch (f)
            {
                case Function.AllLightsOff:
                    return "AllLightsOff";
                case Function.AllLightsOn:
                    return "AllLightsOn";
                case Function.AllUnitsOff:
                    return "AllUnitsOff";
                case Function.Bright:
                    return "Bright";
                case Function.Dim:
                    return "Dim";
                case Function.ExtededDataTransfer:
                    return "ExtededDataTransfer";
                case Function.ExtendedCode:
                    return "ExtendedCode";
                case Function.HailAcknowledge:
                    return "HailAcknowledge";
                case Function.HailRequest:
                    return "HailRequest";
                case Function.Off:
                    return "Off";
                case Function.On:
                    return "On";
                case Function.PresetDim1:
                    return "PresetDim1";
                case Function.PresetDim2:
                    return "PresetDim2";
                case Function.StatusOff:
                    return "StatusOff";
                case Function.StatusOn:
                    return "StatusOn";
                case Function.StatusRequest:
                    return "StatusRequest";
            }
            throw (new ApplicationException("Function out of range"));
        }

        public static Function GetFunctionFromName(string f)
        {
            switch (f)
            {
                case "AllLightsOff":
                    return Function.AllLightsOff;
                case "AllLightsOn":
                    return Function.AllLightsOn;
                case "AllUnitsOff":
                    return Function.AllUnitsOff;
                case "Bright":
                    return Function.Bright;
                case "Dim":
                    return Function.Dim;
                case "ExtededDataTransfer":
                    return Function.ExtededDataTransfer;
                case "ExtendedCode":
                    return Function.ExtendedCode;
                case "HailAcknowledge":
                    return Function.HailAcknowledge;
                case "HailRequest":
                    return Function.HailRequest;
                case "Off":
                    return Function.Off;
                case "On":
                    return Function.On;
                case "PresetDim1":
                    return Function.PresetDim1;
                case "PresetDim2":
                    return Function.PresetDim2;
                case "StatusOff":
                    return Function.StatusOff;
                case "StatusOn":
                    return Function.StatusOn;
                case "StatusRequest":
                    return Function.StatusRequest;
            }
            throw (new ApplicationException("Function out of range"));
        }
    }
}
