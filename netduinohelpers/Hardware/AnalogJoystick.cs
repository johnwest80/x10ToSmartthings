using System;
using Microsoft.SPOT.Hardware;
using netduino.helpers.Math;
using Math = Microsoft.SPOT.Math;
using Microsoft.SPOT.Hardware;

namespace netduino.helpers.Hardware {
    /// <summary>
    /// This class interfaces with an analog x, y joystick such as a the ones found in game controllers (PS/2, Xbox 360, etc.)
    /// 
    /// Tested with a thumb joystick by Sparkfun: http://www.sparkfun.com/products/9032
    /// Datasheet: http://www.p3america.com/pp/pdfs/802.PDF
    /// </summary>
    public class AnalogJoystick : IDisposable
    {
        protected AnalogInput Xinput;
        protected AnalogInput Yinput;

        /// <summary>
        /// Returns the current raw x position
        /// </summary>
        public int X { get { return (int)Xinput.Read(); } }

        /// <summary>
        /// Returns the current raw y position
        /// </summary>
        public int Y { get { return (int)Yinput.Read(); } }

        // Upper end of of the user-defined value range for the X and Y axis
        protected int MaxRange;

        // Defines a relative area around the center of the joystick where the values fluctuate constantly and should be treated as 'center'.
        protected int CenterDeadZoneRadius;

        /// <summary>
        /// Auto-calibration min and max values defining the logical center of the X axis
        /// </summary>
        protected int XMinCenter { get; set; }
        protected int XMaxCenter { get; set; }

        /// <summary>
        /// Auto-calibration min and max values defining the logical center of the Y axis
        /// </summary>
        protected int YMinCenter { get; set; }
        protected int YMaxCenter { get; set; }

        /// <summary>
        /// Relative direction definitions
        /// </summary>
        public enum Direction
        {
            Center = 0,
            Negative = -1,
            Positive = 1
        }

        /// <summary>
        /// Returns the relative direction in which the joystick is moving on the X axis
        /// </summary>
        public Direction XDirection {
            get
            {
                int tempX = X;

                if (tempX >= XMinCenter && tempX <= XMaxCenter)
                {
                    return Direction.Center;
                }

                if (tempX < XMinCenter)
                {
                    return Direction.Negative;
                }

                if (tempX > XMaxCenter)
                {
                    return Direction.Positive;
                }

                return Direction.Center;
            }
        }

        /// <summary>
        /// Returns the relative direction in which the joystick is moving on the Y axis
        /// </summary>
        public Direction YDirection {
            get
            {
                int tempY = Y;

                if (tempY >= YMinCenter && tempY <= YMaxCenter)
                {
                    return Direction.Center;
                }

                if (tempY < YMinCenter)
                {
                    return Direction.Negative;
                }

                if (tempY > YMaxCenter)
                {
                    return Direction.Positive;
                }

                return Direction.Center;
            }
        }

        public double Angle {
            get {
                return Trigo.Atan2(Y - 512, X - 512);
            }
        }

        public double Amplitude {
            get {
                var x = X - 512;
                var y = Y - 512;
                return Trigo.Sqrt(x*x + y*y);
            }
        }

        /// <summary>
        /// Automatically determines the range of values defining the center for the X and Y axis.
        /// Assumes that the joystick is at the center position on the X & Y axis.
        /// Do not touch the joystick during auto-calibration :)
        /// </summary>
        /// <param name="centerDeadZoneRadius">A user-defined radius used to eliminate spurious readings around the center</param>
        public void AutoCalibrateCenter(int centerDeadZoneRadius)
        {
            XMinCenter = X;
            XMaxCenter = XMinCenter;
            YMinCenter = Y;
            YMaxCenter = YMinCenter;

            for (var I = 0; I < 100; I++)
            {
                var tempX = X;
                var tempY = Y;

                if (tempX < XMinCenter)
                {
                    XMinCenter = tempX;
                }

                if (tempX > XMaxCenter)
                {
                    XMaxCenter = tempX;
                }

                if (tempY < YMinCenter)
                {
                    YMinCenter = tempY;
                }

                if (tempY > YMaxCenter)
                {
                    YMaxCenter = tempY;
                }
            }

            XMinCenter -= centerDeadZoneRadius;
            YMaxCenter += centerDeadZoneRadius;
        }

        /// <summary>
        /// Expects two analog pins connected to the x & y axis of the joystick.
        /// </summary>
        /// <param name="xAxisPin">Analog pin for the x axis</param>
        /// <param name="yAxisPin">Analog pin for the y axis</param>
        /// <param name="minRange"></param>
        /// <param name="maxRange"></param>
        /// <param name="centerDeadZoneRadius"></param>
        public AnalogJoystick(Cpu.Pin xAxisPin, Cpu.Pin yAxisPin, int minRange = 0, int maxRange = 1023, int centerDeadZoneRadius = 10)
        {
            MaxRange = maxRange;

            //Xinput = new AnalogInput(xAxisPin);
            //Xinput.SetRange(minRange, maxRange);

            //Yinput = new AnalogInput(yAxisPin);
            //Yinput.SetRange(minRange, maxRange);

            AutoCalibrateCenter(centerDeadZoneRadius);
        }

        /// <summary>
        /// Frees the resources allocated for reading values from the analog joystick
        /// </summary>
        public void Dispose()
        {
            Xinput.Dispose();
            Xinput = null;
            
            Yinput.Dispose();
            Yinput = null;
        }
    }
}
