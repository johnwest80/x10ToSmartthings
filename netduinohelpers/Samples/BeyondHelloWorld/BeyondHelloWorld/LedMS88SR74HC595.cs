using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Hardware;

namespace POV.Matrix {
    /*
    Copyright (C) 2011 by Fabien Royer
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
    */
    /// <summary>
    /// Interface wrapper for the 8*8 LEDMS88R matrix build by Futurlec
    /// Specifications: http://www.futurlec.com/LED/LEDMS88R.shtml
    /// </summary>
    public class LedMS88SR74HC595 : LedMatrix {
        protected OutputPort[] RowPortMap;
        protected ShiftRegister74HC595 ShiftRegister;

        public LedMS88SR74HC595() :
            this(new ShiftRegister74HC595(Pins.GPIO_PIN_D8)) {
        }

        public LedMS88SR74HC595(ShiftRegister74HC595 shiftreg)
            : this(shiftreg, new[] {
                new OutputPort(Pins.GPIO_PIN_D0, true),
                new OutputPort(Pins.GPIO_PIN_D1, true),
                new OutputPort(Pins.GPIO_PIN_D2, true),
                new OutputPort(Pins.GPIO_PIN_D3, true),
                new OutputPort(Pins.GPIO_PIN_D4, true),
                new OutputPort(Pins.GPIO_PIN_D5, true),
                new OutputPort(Pins.GPIO_PIN_D6, true),
                new OutputPort(Pins.GPIO_PIN_D7, true)
            }) {
        }

        public LedMS88SR74HC595(ShiftRegister74HC595 shiftreg, OutputPort[] rowPorts) {
            ShiftRegister = shiftreg;
            RowPortMap = rowPorts;
        }

        public override void OnRow(int logicalRow, byte bitmap, bool energize) {
            // Energize the row in the LED matrix corresponding to the row in the bitmap matrix
            if (energize) {
                ShiftRegister.Write(bitmap);
            }
            RowPortMap[logicalRow].Write(!energize);
        }
    }
}