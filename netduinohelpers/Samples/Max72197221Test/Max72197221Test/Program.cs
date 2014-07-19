using System.Collections;
using System.Threading;
using netduino.helpers.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Max72197221Test {
    public class Program {
        private static Max72197221 _max;
        private static ArrayList _displayList;
        private static byte _intensity;

        public static void Main() {
            InitializeSpinnerDisplay();

            _max = new Max72197221(chipSelect: Pins.GPIO_PIN_D8);
            
            DisplayTestMode();

            ShutdownTestMode();

            DigitScanLimitTest();

            DigitDecodeTest();

            StringToDigitTest();

            var spinnerThread = new Thread(Spinner);
            spinnerThread.Start(); 
            
            var intensityThread = new Thread(IntensityTest);
            intensityThread.Start();

            WaitForEver();
        }

        private static void WaitForEver() {
            while (true) {
                Thread.Sleep(10000);
            }
        }

        private static void StringToDigitTest() {
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.DecodeDigitAll);
            _max.Display("        ");
            Thread.Sleep(1000);
            _max.Display("12345678");
            Thread.Sleep(3000);
            _max.Display("-6.5.4.3.2.1.0.");
            Thread.Sleep(3000);
            _max.Display("z-z-z-z-");
            Thread.Sleep(3000);
            _max.Display("-.-.-. . . . .");
            Thread.Sleep(3000);
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
        }

        private static void DigitDecodeTest() {
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.DecodeDigitAll);
            _max.Display(Max72197221.RegisterAddressMap.Digit0, Max72197221.CodeBFont.Zero, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit1, Max72197221.CodeBFont.One, Max72197221.CodeBDecimalPoint.OFF);
            _max.Display(Max72197221.RegisterAddressMap.Digit2, Max72197221.CodeBFont.Two, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit3, Max72197221.CodeBFont.Three, Max72197221.CodeBDecimalPoint.OFF);
            _max.Display(Max72197221.RegisterAddressMap.Digit4, Max72197221.CodeBFont.Four, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit5, Max72197221.CodeBFont.Five, Max72197221.CodeBDecimalPoint.OFF);
            _max.Display(Max72197221.RegisterAddressMap.Digit6, Max72197221.CodeBFont.Six, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit7, Max72197221.CodeBFont.Seven, Max72197221.CodeBDecimalPoint.OFF);
            Thread.Sleep(4000);
            _max.Display(Max72197221.RegisterAddressMap.Digit0, Max72197221.CodeBFont.Eight, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit1, Max72197221.CodeBFont.Nine, Max72197221.CodeBDecimalPoint.OFF);
            _max.Display(Max72197221.RegisterAddressMap.Digit2, Max72197221.CodeBFont.Dash, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit3, Max72197221.CodeBFont.E, Max72197221.CodeBDecimalPoint.OFF);
            _max.Display(Max72197221.RegisterAddressMap.Digit4, Max72197221.CodeBFont.H, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit5, Max72197221.CodeBFont.L, Max72197221.CodeBDecimalPoint.OFF);
            _max.Display(Max72197221.RegisterAddressMap.Digit6, Max72197221.CodeBFont.P, Max72197221.CodeBDecimalPoint.ON);
            _max.Display(Max72197221.RegisterAddressMap.Digit7, Max72197221.CodeBFont.Blank, Max72197221.CodeBDecimalPoint.OFF);
            Thread.Sleep(4000);
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
        }

        private static void ShutdownTestMode() {
            _max.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            _max.SetDigitScanLimit(7);
            _max.SetIntensity(3);

            _max.Display(new byte[] { 255, 129, 189, 165, 165, 189, 129, 255});

            for(int I = 0; I < 3; I++) {
                Thread.Sleep(300); 
                _max.Shutdown();
                Thread.Sleep(300);
                _max.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
            }
        }

        private static void DigitScanLimitTest() {
            _max.DigitScanLimitSafety = false;
            _max.SetIntensity(1);

            for (int I = 0; I < 3; I++) {
                byte limit = 7;
                for (; limit > 1; limit--) {
                    _max.SetDigitScanLimit(limit);
                    Thread.Sleep(80);
                }
                for (; limit < 8; limit++) {
                    _max.SetDigitScanLimit(limit);
                    Thread.Sleep(80);
                }
            }
            _max.DigitScanLimitSafety = true;
            _max.SetIntensity(3);
        }

        private static void DisplayTestMode() {
            _max.SetDisplayTest(Max72197221.DisplayTestRegister.DisplayTestMode);
            Thread.Sleep(4000);
            _max.SetDisplayTest(Max72197221.DisplayTestRegister.NormalOperation);
        }

        private static void InitializeSpinnerDisplay() {
            _displayList = new ArrayList {
                                             new byte[] {1, 2, 4, 8, 16, 32, 64, 128},
                                             new byte[] {0, 0, 0, 255, 0, 0, 0, 0},
                                             new byte[] {128, 64, 32, 16, 8, 4, 2, 1},
                                             new byte[] {16, 16, 16, 16, 16, 16, 16, 16}
                                         };
        }

        private static void IntensityTest() {
            while(true) {
                for (_intensity = 0; _intensity <= 15; _intensity++) {
                    Thread.Sleep(80);
                }
                for (_intensity = 15; _intensity != 255; _intensity--) {
                    Thread.Sleep(80);
                }
            }
        }

        private static void Spinner() {
            while (true) {
                foreach (byte[] matrix in _displayList) {
                    _max.Display(matrix);
                    _max.SetIntensity(_intensity);
                    Thread.Sleep(80);
                }
            }
        }
    }
}
