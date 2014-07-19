#define NETDUINO

using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.IO;

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
using SecretLabs.NETMF.Hardware.Netduino;
#endif

using SecretLabs.NETMF.Hardware;
using netduino.helpers.Sound;
using netduino.helpers.Helpers;

namespace RingToneTest {
    public class Program {
        static PWM _channel = new PWM(Pins.GPIO_PIN_D5);
        public static readonly string SDMountPoint = @"SD";

        public static void Main() {
            AsynchronousPlay();

            Debug.EnableGCMessages(true);
            Debug.Print(Debug.GC(true).ToString());

            PlayFromResource();

            Debug.Print(Debug.GC(true).ToString());
            Debug.EnableGCMessages(false);
        }

        /// <summary>
        /// Play an RTTL song asynchronously
        /// </summary>
        public static void AsynchronousPlay() {
            var song = new RttlSong("PacMan:d=4,o=5,b=90:32b,32p,32b6,32p,32f#6,32p,32d#6,32p,32b6,32f#6,16p,16d#6,16p,32c6,32p,32c7,32p,32g6,32p,32e6,32p,32c7,32g6,16p,16e6,16p,32b,32p,32b6,32p,32f#6,32p,32d#6,32p,32b6,32f#6,16p,16d#6,16p,32d#6,32e6,32f6,32p,32f6,32f#6,32g6,32p,32g6,32g#6,32a6,32p,32b.6");
            
            var thread = song.Play(_channel, true);

            for (int I = 0; I < song.Duration; I++) {
                Debug.Print("Playing:" + song.Name + "...");
                Thread.Sleep(1000);
            }

            thread.Join();
        }

        /// <summary>
        /// Play a set of RTTL songs synchronously, loaded from SD card resources
        /// </summary>
        public static void PlayFromResource(){
#if NETDUINO_MINI
            StorageDevice.MountSD(SDMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#else
            StorageDevice.MountSD(SDMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif
            var sd = new SDResourceLoader();
            sd.Load();

            foreach(string songName in sd.RTTLSongs.Keys) {
                RttlSong song = (RttlSong) sd.RTTLSongs[songName];
                Debug.Print("Playing: " + song.Name);
                song.Play(_channel);
                Thread.Sleep(1500);
            }

#if NETDUINO_MINI || NETDUINO
            StorageDevice.Unmount(SDMountPoint);
#endif
        }
    }
}