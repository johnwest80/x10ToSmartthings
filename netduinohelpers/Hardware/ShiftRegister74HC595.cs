using Microsoft.SPOT.Hardware;
using netduino.helpers.Helpers;

namespace netduino.helpers.Hardware {
    /// <summary>
    /// Abstracts a 74HC595 shift register accessed through SPI
    /// </summary>
    public class ShiftRegister74HC595 {
        protected SPI Spi;

        /// <summary>
        /// By default, SPI expects the clock signal (74HC595 SRCLK) on pin 13 and data (74HC595 SER) on the on pin 11. The latch pin is arbitrary.
        /// By default SPI1 is used on the netduino
        /// </summary>
        /// <param name="latchPin">Pin connected to register latch (called ST_CP or RCLK) of the 74HC595</param>
        public ShiftRegister74HC595(Cpu.Pin latchPin)
            : this(latchPin, SPI.SPI_module.SPI1) {
        }

        /// <summary>
        /// Code for using a 74HC595 Shift Register
        /// </summary>
        /// <param name="latchPin">Pin connected to register latch on the 74HC595</param>
        /// <param name="spiModule">SPI module being used to send data to the shift register</param>
        public ShiftRegister74HC595(Cpu.Pin latchPin, SPI.SPI_module spiModule, uint speedKHz = 1000) {
            var spiConfig = new SPI.Configuration(
                SPI_mod: spiModule,
                ChipSelect_Port: latchPin,
                ChipSelect_ActiveState: false,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: false,
                Clock_Edge: true,
                Clock_RateKHz: speedKHz
                );
            Spi = new SPI(spiConfig);
        }
        /// <summary>
        /// Sends 8 bits to the shift register
        /// </summary>
        /// <param name="buffer"></param>
        public void Write(byte buffer) {
            Spi.Write(new[] {buffer});
        }

        /// <summary>
        /// Reverse the bits of the byte
        /// </summary>
        /// <param name="val">A byte value to be reversed</param>
        /// <returns>The byte with the reversed bits</returns>
        public byte FlipBits(byte val)
        {
            return BitReverseTable256.Table[val];
        }
    }
}