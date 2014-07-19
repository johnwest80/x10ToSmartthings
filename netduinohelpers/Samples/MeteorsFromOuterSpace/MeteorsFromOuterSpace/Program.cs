//#define dev
using Microsoft.SPOT.Hardware;
using netduino.helpers.Fun;
using netduino.helpers.Hardware;
using netduino.helpers.Helpers;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Meteors {
    public class Program {
        /// <summary>
        /// During development, Main() acts as the ConsoleBootLoader, making it easy to debug the game.
        /// When game development is complete, comment out the content Main() to remove the overhead
        /// </summary>
        public static void Main() {
#if dev
            var joystickLeft = new AnalogJoystick(xAxisPin: Pins.GPIO_PIN_A0, yAxisPin: Pins.GPIO_PIN_A1);
            var joystickRight = new AnalogJoystick(xAxisPin: Pins.GPIO_PIN_A2, yAxisPin: Pins.GPIO_PIN_A3);
            var matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_D8);
            var speaker = new PWM(Pins.GPIO_PIN_D5);
            var resourceLoader = new SDResourceLoader();
            var buttonLeft = new PushButton(Pins.GPIO_PIN_D0, Port.InterruptMode.InterruptEdgeLevelLow, null, Port.ResistorMode.PullUp);
            var buttonRight = new PushButton(Pins.GPIO_PIN_D1, Port.InterruptMode.InterruptEdgeLevelLow, null, Port.ResistorMode.PullUp);
            var args = new object[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.Size];

            var index = 0;
            args[index++] = CartridgeVersionInfo.CurrentVersion;
            args[index++] = joystickLeft;
            args[index++] = joystickRight;
            args[index++] = matrix;
            args[index++] = speaker;
            args[index++] = resourceLoader;
            args[index++] = buttonLeft;
            args[index] = buttonRight;

            matrix.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
            matrix.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            matrix.SetDigitScanLimit(7);
            matrix.SetIntensity(8);

            Run(args);
#endif
        }

        /// <summary>
        /// Entry point called by the ConsoleBootLoader project
        /// </summary>
        /// <param name="args">Array of object references to the hardware features</param>
        public static void Run(object[] args) {
            var thread = new GameOfMeteors(new ConsoleHardwareConfig(args)).Run();
            thread.Join();
        }
    }
}