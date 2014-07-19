using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Hardware {
    /// <summary>
    /// MAX7219 / 7221 LED display driver
    /// http://datasheets.maxim-ic.com/en/ds/MAX7219-MAX7221.pdf
    /// </summary>
    public class Max72197221 : IDisposable {
        public enum RegisterAddressMap {
            NoOp,
            Digit0,
            Digit1,
            Digit2,
            Digit3,
            Digit4,
            Digit5,
            Digit6,
            Digit7,
            DecodeMode,
            Intensity,
            ScanLimit,
            Shutdown,
            DisplayTest = 0x0F
        }

        public enum ShutdownRegister {
            ShutdownMode,
            NormalOperation
        }

        /// <summary>
        /// Logic OR the values of the DecodeModeRegister together or use DecodeDigitAll to decode all digits
        /// </summary>
        [Flags]
        public enum DecodeModeRegister {
            NoDecodeMode,
            DecodeDigit0,
            DecodeDigit1,
            DecodeDigit2,
            DecodeDigit3,
            DecodeDigit4,
            DecodeDigit5,
            DecodeDigit6,
            DecodeDigit7,
            DecodeDigitAll = 255
        }

        public enum CodeBFont {
            Zero,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Dash,
            E,
            H,
            L,
            P,
            Blank = 0x0F
        }

        public enum CodeBDecimalPoint {
            OFF,
            ON = 0x80
        }

        public enum DisplayTestRegister {
            NormalOperation,
            DisplayTestMode
        }

        /// <summary>
        /// Instantiates a Max7219/7221 LED driver, using the netduino's hard SPI interface by default:
        /// If multiple Max7219/7221 chips are chained together, the CS pin must be controlled by the caller instead of the netduino handling it.
        /// CLK = pin 13
        /// MOSI = pin 11
        /// CS = pin 10
        /// </summary>
        /// <param name="chipSelect">Chip Select pin.</param>
        /// <param name="spiModule">SPI module, SPI 1 is used by default.</param>
        /// <param name="speedKHz">Speed of the SPI bus in kHz. Set @ 10MHz by default (max chip speed).</param>
        public Max72197221(Cpu.Pin chipSelect, SPI.SPI_module spiModule = SPI.SPI_module.SPI1, uint speedKHz = (uint)10000) {
            var extendedSpiConfig = new ExtendedSpiConfiguration(
                SPI_mod: spiModule,
                ChipSelect_Port: chipSelect,
                ChipSelect_ActiveState: false,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: false,
                Clock_Edge: true,
                Clock_RateKHz: speedKHz,
                BitsPerTransfer: 16);

            Spi = new SPI(extendedSpiConfig);

            DigitScanLimitSafety = true;

            SpiBuffer = new ushort[1];
        }

        public void SetIntensity(byte value) {
            if (value < 0 || value > 15) {
                throw new ArgumentOutOfRangeException("value");
            }
            Write((byte) RegisterAddressMap.Intensity, value);
        }

        public bool DigitScanLimitSafety { get; set; }

        public void SetDigitScanLimit(byte value) {
            if (value < 0 || value > 7) {
                throw new ArgumentOutOfRangeException("value");
            }

            if (DigitScanLimitSafety && value < 3) {
                throw new ArgumentException("SetDigitScanLimitSafety value should not be set too low in order to keep within datasheet limits and protect your matrix or digits from burning out.");
            }
            Write((byte) RegisterAddressMap.ScanLimit, value);
        }

        public void SetDecodeMode(DecodeModeRegister value) {
            if (value < DecodeModeRegister.NoDecodeMode || value > DecodeModeRegister.DecodeDigitAll) {
                throw new ArgumentOutOfRangeException("value");
            }
            Write((byte) RegisterAddressMap.DecodeMode, (byte) value);
        }

        public void Shutdown(ShutdownRegister value = ShutdownRegister.ShutdownMode) {
            if (value != ShutdownRegister.NormalOperation && value != ShutdownRegister.ShutdownMode) {
                throw new ArgumentOutOfRangeException("value");
            }
            Write((byte) RegisterAddressMap.Shutdown, (byte) value);
        }

        public void SetDisplayTest(DisplayTestRegister value) {
            if (value != DisplayTestRegister.DisplayTestMode && value != DisplayTestRegister.NormalOperation) {
                throw new ArgumentOutOfRangeException("value");
            }
            Write((byte) RegisterAddressMap.DisplayTest, (byte) value);
        }

        /// <summary>
        /// Send an 8x8 matrix pattern to the LED driver.
        /// The LED driver requires DecodeMode = DecodeModeRegister.NoDecodeMode first.
        /// </summary>
        /// <param name="matrix">8x8 bitmap to be displayed.</param>
        public void Display(byte[] matrix) {
            if (matrix.Length != 8) {
                throw new ArgumentOutOfRangeException("matrix");
            }
            var rowNumber = (byte)RegisterAddressMap.Digit0;
            foreach (var rowData in matrix) {
                Write(rowNumber, rowData);
                rowNumber++;
            }
        }

        /// <summary>
        /// Translate a string into CodeBFont values and displays it.
        /// Each character followed by a '.' will be displayed with a decimal point.
        /// Unrecognized characters will be replaced by a 'blank' CodeBFont value.
        /// The string is scanned from the right to the left (LSB first).
        /// The LED driver requires DecodeMode != DecodeModeRegister.NoDecodeMode first.
        /// </summary>
        /// <param name="digits">A string containing characters from 0-9, '-', ' ', 'E', 'H', 'L', 'P' or '.'</param>
        public void Display(string digits) {
            var length = digits.Length;
            var decimalPoint = CodeBDecimalPoint.OFF;
            var digitPosition = RegisterAddressMap.Digit0;
            while (length != 0) {
                var c = digits[--length];                
                if (c == '.') {
                    decimalPoint = CodeBDecimalPoint.ON;
                    continue;
                }
                CodeBFont data;
                if (c >= '0' && c <= '9') {
                    data = (CodeBFont)(c - '0');
                } else {
                    switch (c) {
                        case '-':
                            data = CodeBFont.Dash;
                            break;
                        case 'E':
                            data = CodeBFont.E;
                            break;
                        case 'H':
                            data = CodeBFont.H;
                            break;
                        case 'L':
                            data = CodeBFont.L;
                            break;
                        case 'P':
                            data = CodeBFont.P;
                            break;
                        default:
                            data = CodeBFont.Blank;
                            break;
                    }
                }

                Display(digitPosition, data, decimalPoint);

                decimalPoint = CodeBDecimalPoint.OFF; 
                
                digitPosition++;
                if (digitPosition > RegisterAddressMap.Digit7) {
                    break;
                }
            }
        }

        /// Send a CodeBFont pattern to a specific digit register of the LED driver. No-op is acceptable.
        /// The LED driver requires DecodeMode != DecodeModeRegister.NoDecodeMode first.
        public void Display(RegisterAddressMap register, CodeBFont codeBFont, CodeBDecimalPoint decimalPoint) {
            if (register < RegisterAddressMap.NoOp || register > RegisterAddressMap.Digit7) {
                throw new ArgumentOutOfRangeException("register");
            }
            var data = (byte)codeBFont;
            data |= (byte)decimalPoint;
            Write((byte)register, data);
        }

        protected void Write(byte register, byte value) {
            SpiBuffer[0] = register;
            SpiBuffer[0] <<= 8;
            SpiBuffer[0] |= value;
            Spi.Write(SpiBuffer);
        }

        public void Dispose() {
            Spi.Dispose();
            Spi = null;
            SpiBuffer = null;
        }

        protected SPI Spi;
        protected ushort[] SpiBuffer;
    }
}
