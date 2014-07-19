#define NETDUINO

using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.IO;

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
using SecretLabs.NETMF.Hardware.Netduino;
#endif

using netduino.helpers.Hardware;
using netduino.helpers.Imaging;
using netduino.helpers.Helpers;
using POV.Matrix;

namespace BeyondHelloWorld
{
    public class Program
    {
        protected static int X = 0;
        protected static int Y = 0;
        protected static PushButton JoystickButton;
        protected static AnalogJoystick Joystick;
        public static readonly string SDMountPoint = @"SD";

        public static void Main()
        {
            SDResourceLoader rsc = null;

            Joystick = new AnalogJoystick(Pins.GPIO_PIN_A0, Pins.GPIO_PIN_A1);

            // I'm being lazy here and using the default on-board switch instead of the actual joystick button :)
            JoystickButton = new PushButton(pin: Pins.ONBOARD_SW1, target: new NativeEventHandler(ButtonEventHandler));

            try {
#if NETDUINO_MINI
                StorageDevice.MountSD(SDMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#else
                StorageDevice.MountSD(SDMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif
                // Load the resources from the SD card 
                // Place the content of the "SD Card Resources" folder at the root of an SD card
                rsc = new SDResourceLoader();
                rsc.Load();

#if NETDUINO_MINI || NETDUINO
                StorageDevice.Unmount(SDMountPoint);
#endif
            }
            catch (IOException) {
                ShowNoSDPresent();
            }

            // Using the space invaders bitmap in this example
            var Invaders = (Bitmap) rsc.Bitmaps["spaceinvaders.bmp.bin"];

            rsc.Dispose();

            var matrix = new LedMS88SR74HC595().Initialize();

            while (true)
            {
                // Read the current direction of the joystick
                X += (int) Joystick.XDirection;
                Y += (int) Joystick.YDirection;

                // Validate the position of the coordinates to prevent out-of-bound exceptions.
                if (X < 0)
                {
                    X = 0;
                }
                else if (X >= Invaders.Width - Bitmap.FrameSize)
                {
                    X = Invaders.Width - Bitmap.FrameSize;
                }

                if (Y < 0)
                {
                    Y = 0;
                }
                else if (Y >= Invaders.Height)
                {
                    Y = Invaders.Height - 1;
                }

                Debug.Print("X=" + Joystick.X.ToString() + " (" + Joystick.XDirection.ToString() + ")" + ", Y=" + Joystick.Y.ToString() + " (" + Joystick.YDirection.ToString() + ")");

                // move the bitmap according to the direction of the joystick
                matrix.Set(Invaders.GetFrame(X, Y));

                Thread.Sleep(80);
            }
        }

        // When the button is pushed, bring back the bitmap to the starting point.
        public static void ButtonEventHandler(UInt32 port, UInt32 state, DateTime time)
        {
            JoystickButton.Input.DisableInterrupt();

            if (state == 1)
            {
                X = 0;
                Y = 0;
            }

            JoystickButton.Input.EnableInterrupt();
        }

        // Shows and SD icon on the matrix and wait for a reset
        private static void ShowNoSDPresent()
        {
            //  If the SD card is not present, show an SD card icon and wait for a reset...
            using (var matrix = new LedMS88SR74HC595().Initialize())
            {
                var SD = new Bitmap(new byte[] { 0x7e, 0x42, 0x42, 0x42, 0x42, 0x42, 0x22, 0x1e }, 8, 8);
                matrix.Set(SD.GetFrame(0, 0));
                while (true) { 
                    Thread.Sleep(1000); 
                }
            }
        }
    }
}