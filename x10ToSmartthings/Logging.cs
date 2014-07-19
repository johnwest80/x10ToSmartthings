using System;
using Microsoft.SPOT;
using netduino.helpers.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace x10tosmartthings
{
    /// <summary>
    /// This is SO hackish.  I should have had a separate class for logging to the screen instead of 
    /// dumping the code into the logging and debug section.  But oh well... sometimes you do things quickly!
    /// </summary>
    public class Logging
    {
        public static AdaFruitSSD1306 Oled = new AdaFruitSSD1306(
            chipSelect: Pins.GPIO_PIN_D8,
            reset: Pins.GPIO_PIN_D9,
            dc: Pins.GPIO_PIN_D10,
            speedKHz: 500);

        private static int _lineNum;
        private static bool _statusDotShowing;

        public static void Init()
        {
            Oled.Initialize();
        }

        public static void DebugPrint(string str)
        {
            for (int i = 0; i < 8; i++)
                Oled.DrawString(1, i, " ");
            Oled.DrawString(1, _lineNum++, "*" + str, false);
            Oled.Refresh();

            if (_lineNum == 8)
                _lineNum = 0;

            Debug.Print(DateTime.Now + ": " + str);
        }

        public static void ToggleStatusDot()
        {
            _statusDotShowing = !_statusDotShowing;
            Oled.SetPixel(127, 63, _statusDotShowing ? AdaFruitSSD1306.Color.White : AdaFruitSSD1306.Color.Black);
            Oled.Refresh();
        }
    }
}