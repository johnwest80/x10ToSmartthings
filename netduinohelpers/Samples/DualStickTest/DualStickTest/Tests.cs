using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Hardware;
using netduino.helpers.Imaging;
using netduino.helpers.Helpers;

namespace DualStickTest
{
    public class Program
    {
        public static void Main()
        {
            var LeftJoystick = new AnalogJoystick(Pins.GPIO_PIN_A0, Pins.GPIO_PIN_A1);
            var RightJoystick = new AnalogJoystick(Pins.GPIO_PIN_A2, Pins.GPIO_PIN_A3);

            var matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_D8);

            matrix.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
            matrix.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            matrix.SetDigitScanLimit(7);
            matrix.SetIntensity(8);

            var comp = new Composition(new byte[8], 8, 8);

            var leftBall = new PlayerMissile("leftBall", 0, 0);
            var rightBall = new PlayerMissile("rightBall", 0, 0);

            comp.AddMissile(leftBall);
            comp.AddMissile(rightBall);

            while (true)
            {
                leftBall.X = LeftJoystick.X / 128;
                leftBall.Y = LeftJoystick.Y / 128;
                rightBall.X = RightJoystick.X / 128;
                rightBall.Y = RightJoystick.Y / 128;

                Debug.Print("X=" + LeftJoystick.X.ToString() + " (" + LeftJoystick.XDirection.ToString() + ")" + ", Y=" + LeftJoystick.Y.ToString() + " (" + LeftJoystick.YDirection.ToString() + ")");

                matrix.Display(comp.GetFrame(0, 0));

                Thread.Sleep(80);
            }
        }
    }
}