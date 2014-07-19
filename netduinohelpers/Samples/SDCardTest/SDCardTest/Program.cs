#define NETDUINO

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
using SecretLabs.NETMF.Hardware.Netduino;
#endif

using SecretLabs.NETMF.IO;
using System.IO;

namespace SDCardTest
{
    public class Program
    {
        public static void Main()
        {

#if NETDUINO_MINI
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#else
            StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif

            using (var filestream = new FileStream(@"SD\resources.txt", FileMode.Open))
            {
                StreamReader reader = new StreamReader(filestream);
                Debug.Print(reader.ReadToEnd());
                reader.Close();
            }

            using (var filestream = new FileStream(@"SD\dontpanic.txt", FileMode.Create))
            {
                StreamWriter streamWriter = new StreamWriter(filestream);
                streamWriter.WriteLine("This is a test of the SD card support on the netduino...This is only a test...");
                streamWriter.Close();
            }

            StorageDevice.Unmount("SD");
        }
    }
}
