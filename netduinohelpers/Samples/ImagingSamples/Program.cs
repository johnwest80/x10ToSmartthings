using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using netduino.helpers.Hardware;
using netduino.helpers.Imaging;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ImagingSamples {
    public class Program {
        public static void Main() {
            using (var matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_D8, speedKHz: 10000)) {

                matrix.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
                matrix.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
                matrix.SetDigitScanLimit(7);
                matrix.SetIntensity(8);

                var comp = new Composition(new byte[]  {
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x03, 0xC0,
                    0x07, 0xE0,
                    0x0F, 0xF0,
                    0x0F, 0xF0,
                    0x0F, 0xF0,
                    0x0F, 0xF0,
                    0x07, 0xE0,
                    0x03, 0xC0,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                    0x00, 0x00,
                }, 16, 16);

                var player = new PlayerMissile("player", 0, 0);
                var missile = new PlayerMissile("missile", 0, 0);
                comp.AddMissile(player);
                comp.AddMissile(missile);

                while (true) {
                    for (var angle = 0; angle < 360; angle++) {
                        player.X = 8 + Math.Sin(angle * 2)/160;
                        player.Y = 8 + Math.Cos(angle * 2)/160;
                        missile.X = 8 + Math.Sin(angle) / 160;
                        missile.Y = 8 + Math.Cos(angle) / 160;
                        var frame = comp.GetFrame(Math.Sin(angle*20)/250 + 4, Math.Cos(angle*20)/250 + 4);
                        matrix.Display(frame);
                        Thread.Sleep(25);
                    }
                }
            }
        }
    }
}
