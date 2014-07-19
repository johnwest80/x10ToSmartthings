#define NETDUINO

using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.IO;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Hardware;

namespace AdaFruitST7735Test {
    public class Program {
        public static AdaFruitST7735 tft = new AdaFruitST7735(Pins.GPIO_PIN_D9, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D8, speedKHz: 40000);

        public static void Main() {
            DisplayPicture();

            tft.ClearScreen();
            DisplayGradient();

            DisplayCircles();

            tft.ClearScreen();
            DisplayLines();

            DisplayColorFlow();
        }

        public static void DisplayColorFlow() {
#if NETDUINO_MINI
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#else
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif
            while (true) {
                ReadPicture(@"SD\Pictures\ColorFlow1.bmp.24.bin", 0);
                ReadPicture(@"SD\Pictures\ColorFlow2.bmp.24.bin", 0);
                ReadPicture(@"SD\Pictures\ColorFlow3.bmp.24.bin", 0);
                ReadPicture(@"SD\Pictures\ColorFlow4.bmp.24.bin", 0);
            }
        }

        public static void DisplayPicture() {
#if NETDUINO_MINI
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#else
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif
            ReadPicture(@"SD\Pictures\spaceneedle.bmp.24.bin");
            ReadPicture(@"SD\Pictures\spaceneedleclose.bmp.24.bin");
            ReadPicture(@"SD\Pictures\spaceneedlesunset.bmp.24.bin");
            ReadPicture(@"SD\Pictures\spaceneedleatnight.bmp.24.bin");
            
            StorageDevice.Unmount("SD");
        }

        public static void ReadPicture(string filename, int delay = 1000) {
            using (var filestream = new FileStream(filename, FileMode.Open)) {
                filestream.Read(tft.SpiBuffer, 0, tft.SpiBuffer.Length);
                tft.Refresh();
                Thread.Sleep(delay);
            }
        }

        public static void DisplayLines() {
            byte red = 20;
            byte green = 1;
            byte blue = 5;
            var y = 0;
            
            for (; y < AdaFruitST7735.Height; y++) {
                red += 2;
                green++;
                tft.DrawLine(0, 0, AdaFruitST7735.Width, y, tft.GetRGBColor(red, green, blue));
                tft.Refresh();
            }

            red = 20;
            green = 1;
            blue = 5;
            for (; y >= 0; y--) {
                red += 2;
                green++;
                tft.DrawLine(AdaFruitST7735.Width - 1, AdaFruitST7735.Height - 1, 0, y, tft.GetRGBColor(red, green, blue));
                tft.Refresh();
            }
        }

        public static void DisplayCircles() {
            var xHalf = AdaFruitST7735.Width / 2;
            var yHalf = AdaFruitST7735.Height / 2;
            byte red = 1;
            byte green = 1;
            byte blue = 1;        

            for (var r = 1; r < xHalf; r+=2) {
                var color = tft.GetRGBColor(red, green, blue);
                tft.DrawCircle(xHalf, yHalf, r, color);
                red += 3;
                green += 2;
                blue += 1;
                tft.Refresh();
            }

            Thread.Sleep(1000);

            for (var I = 0; I < 2; I++) {
                var r = 1;
                for (; r < xHalf; r += 2) {
                    tft.DrawCircle(xHalf, yHalf, r, (ushort)AdaFruitST7735.Colors.White);
                    tft.Refresh();
                    tft.DrawCircle(xHalf, yHalf, r, (ushort)AdaFruitST7735.Colors.Black);
                }
                for (; r > 1; r -= 2) {
                    tft.DrawCircle(xHalf, yHalf, r, (ushort)AdaFruitST7735.Colors.White);
                    tft.Refresh();
                    tft.DrawCircle(xHalf, yHalf, r, (ushort)AdaFruitST7735.Colors.Black);
                }
            }

            Thread.Sleep(1000);
        }

        public static void DisplayGradient(){
            var x = 0;
            var y = 0;

            while (y < AdaFruitST7735.Height) {
                byte red = 1;
                for (; red < 32; red+=3) {
                    byte green = 1;
                    for (; green < 33; green+=2) {
                        byte blue = 1;
                        for (; blue < 32; blue+=2) {
                            var color = tft.GetRGBColor(red, green, blue);

                            tft.DrawPixel(x++, y, color);

                            if (x >= AdaFruitST7735.Width) {
                                x = 0;
                                y++;
                            }
                        }
                    }
                }
                tft.Refresh();
            }
        }
    }
}
