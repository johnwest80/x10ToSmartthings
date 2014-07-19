using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace netduino.helpers.Hardware {
    /// <summary>
    /// Graphics library by ladyada/adafruit with init code from Rossum 
    /// MIT license
    /// https://github.com/adafruit/TFTLCD-Library/
    /// C# port by Fabien Royer (work in progress)
    /// </summary>
    class AdaFruitILI9328
    {
        private enum Register {
            TFTLCD_START_OSC             = 0x00,
            TFTLCD_DRIV_OUT_CTRL		 = 0x01,
            TFTLCD_DRIV_WAV_CTRL		 = 0x02,
            TFTLCD_ENTRY_MOD			 = 0x03,
            TFTLCD_RESIZE_CTRL			 = 0x04,
            TFTLCD_DISP_CTRL1			 = 0x07,
            TFTLCD_DISP_CTRL2			 = 0x08,
            TFTLCD_DISP_CTRL3			 = 0x09,
            TFTLCD_DISP_CTRL4			 = 0x0A,
            TFTLCD_RGB_DISP_IF_CTRL1	 = 0x0C,
            TFTLCD_FRM_MARKER_POS		 = 0x0D,
            TFTLCD_RGB_DISP_IF_CTRL2	 = 0x0F,
            TFTLCD_POW_CTRL1			 = 0x10,
            TFTLCD_POW_CTRL2			 = 0x11,
            TFTLCD_POW_CTRL3			 = 0x12,
            TFTLCD_POW_CTRL4			 = 0x13,
            TFTLCD_GRAM_HOR_AD			 = 0x20,
            TFTLCD_GRAM_VER_AD			 = 0x21,
            TFTLCD_RW_GRAM				 = 0x22,
            TFTLCD_POW_CTRL7			 = 0x29,
            TFTLCD_FRM_RATE_COL_CTRL	 = 0x2B,
            TFTLCD_GAMMA_CTRL1			 = 0x30,
            TFTLCD_GAMMA_CTRL2			 = 0x31,
            TFTLCD_GAMMA_CTRL3			 = 0x32,
            TFTLCD_GAMMA_CTRL4			 = 0x35,
            TFTLCD_GAMMA_CTRL5			 = 0x36,
            TFTLCD_GAMMA_CTRL6			 = 0x37,
            TFTLCD_GAMMA_CTRL7			 = 0x38,
            TFTLCD_GAMMA_CTRL8			 = 0x39,
            TFTLCD_GAMMA_CTRL9			 = 0x3C,
            TFTLCD_GAMMA_CTRL10			 = 0x3D,
            TFTLCD_HOR_START_AD			 = 0x50,
            TFTLCD_HOR_END_AD			 = 0x51,
            TFTLCD_VER_START_AD			 = 0x52,
            TFTLCD_VER_END_AD			 = 0x53,
            TFTLCD_GATE_SCAN_CTRL1		 = 0x60,
            TFTLCD_GATE_SCAN_CTRL2		 = 0x61,
            TFTLCD_GATE_SCAN_CTRL3		 = 0x6A,
            TFTLCD_PART_IMG1_DISP_POS	 = 0x80,
            TFTLCD_PART_IMG1_START_AD	 = 0x81,
            TFTLCD_PART_IMG1_END_AD		 = 0x82,
            TFTLCD_PART_IMG2_DISP_POS	 = 0x83,
            TFTLCD_PART_IMG2_START_AD	 = 0x84,
            TFTLCD_PART_IMG2_END_AD		 = 0x85,
            TFTLCD_PANEL_IF_CTRL1		 = 0x90,
            TFTLCD_PANEL_IF_CTRL2		 = 0x92,
            TFTLCD_PANEL_IF_CTRL3		 = 0x93,
            TFTLCD_PANEL_IF_CTRL4		 = 0x95,
            TFTLCD_PANEL_IF_CTRL5		 = 0x97,
            TFTLCD_PANEL_IF_CTRL6		 = 0x98
        }

        private static readonly ushort _TFTLCD_DELAYCMD = 0xFF;

        private static readonly ushort[] _initializationSequence = new ushort[] {
            (ushort) Register.TFTLCD_START_OSC, 0x0001,     // start oscillator
            _TFTLCD_DELAYCMD, 50,                           // this will make a delay of 50 milliseconds
            (ushort) Register.TFTLCD_DRIV_OUT_CTRL, 0x0100, 
            (ushort) Register.TFTLCD_DRIV_WAV_CTRL, 0x0700,
            (ushort) Register.TFTLCD_ENTRY_MOD, 0x1030,
            (ushort) Register.TFTLCD_RESIZE_CTRL, 0x0000,
            (ushort) Register.TFTLCD_DISP_CTRL2, 0x0202,
            (ushort) Register.TFTLCD_DISP_CTRL3, 0x0000,
            (ushort) Register.TFTLCD_DISP_CTRL4, 0x0000,
            (ushort) Register.TFTLCD_RGB_DISP_IF_CTRL1, 0x0,
            (ushort) Register.TFTLCD_FRM_MARKER_POS, 0x0,
            (ushort) Register.TFTLCD_RGB_DISP_IF_CTRL2, 0x0,
            (ushort) Register.TFTLCD_POW_CTRL1, 0x0000,
            (ushort) Register.TFTLCD_POW_CTRL2, 0x0007,
            (ushort) Register.TFTLCD_POW_CTRL3, 0x0000,
            (ushort) Register.TFTLCD_POW_CTRL4, 0x0000,
            _TFTLCD_DELAYCMD, 200,
            (ushort) Register.TFTLCD_POW_CTRL1, 0x1690,
            (ushort) Register.TFTLCD_POW_CTRL2, 0x0227,
            _TFTLCD_DELAYCMD, 50,
            (ushort) Register.TFTLCD_POW_CTRL3, 0x001A,
            _TFTLCD_DELAYCMD, 50,
            (ushort) Register.TFTLCD_POW_CTRL4, 0x1800,
            (ushort) Register.TFTLCD_POW_CTRL7, 0x002A,
            _TFTLCD_DELAYCMD,50,
            (ushort) Register.TFTLCD_GAMMA_CTRL1, 0x0000,    
            (ushort) Register.TFTLCD_GAMMA_CTRL2, 0x0000, 
            (ushort) Register.TFTLCD_GAMMA_CTRL3, 0x0000,
            (ushort) Register.TFTLCD_GAMMA_CTRL4, 0x0206,   
            (ushort) Register.TFTLCD_GAMMA_CTRL5, 0x0808,  
            (ushort) Register.TFTLCD_GAMMA_CTRL6, 0x0007,  
            (ushort) Register.TFTLCD_GAMMA_CTRL7, 0x0201,
            (ushort) Register.TFTLCD_GAMMA_CTRL8, 0x0000,  
            (ushort) Register.TFTLCD_GAMMA_CTRL9, 0x0000,  
            (ushort) Register.TFTLCD_GAMMA_CTRL10, 0x0000,  
            (ushort) Register.TFTLCD_GRAM_HOR_AD, 0x0000,  
            (ushort) Register.TFTLCD_GRAM_VER_AD, 0x0000,  
            (ushort) Register.TFTLCD_HOR_START_AD, 0x0000,
            (ushort) Register.TFTLCD_HOR_END_AD, 0x00EF,
            (ushort) Register.TFTLCD_VER_START_AD, 0X0000,
            (ushort) Register.TFTLCD_VER_END_AD, 0x013F,
            (ushort) Register.TFTLCD_GATE_SCAN_CTRL1, 0xA700,     // Driver Output Control (R60h)
            (ushort) Register.TFTLCD_GATE_SCAN_CTRL2, 0x0003,     // Driver Output Control (R61h)
            (ushort) Register.TFTLCD_GATE_SCAN_CTRL3, 0x0000,     // Driver Output Control (R62h)
            (ushort) Register.TFTLCD_PANEL_IF_CTRL1, 0X0010,     // Panel Interface Control 1 (R90h)
            (ushort) Register.TFTLCD_PANEL_IF_CTRL2, 0X0000,
            (ushort) Register.TFTLCD_PANEL_IF_CTRL3, 0X0003,
            (ushort) Register.TFTLCD_PANEL_IF_CTRL4, 0X1100,
            (ushort) Register.TFTLCD_PANEL_IF_CTRL5, 0X0000,
            (ushort) Register.TFTLCD_PANEL_IF_CTRL6, 0X0000,
            (ushort) Register.TFTLCD_DISP_CTRL1, 0x0133     // Display Control (R07h) - Display ON
        };

        private OutputPort _chipSelect;
        private OutputPort _commandData;
        private OutputPort _write;
        private OutputPort _read;
        private OutputPort _reset;
        private ShiftRegister74HC595 _data;

        public AdaFruitILI9328(ShiftRegister74HC595 data, Cpu.Pin chipSelect, Cpu.Pin commandData, Cpu.Pin write, Cpu.Pin read, Cpu.Pin reset) {
            _chipSelect = new OutputPort(chipSelect, false);
            _commandData = new OutputPort(commandData, false);
            _write = new OutputPort(write, false);
            _read = new OutputPort(read, false);
            _reset = new OutputPort(reset, false);
        }

        public void Initialize() {
            Reset();

            for (var i = 0; i < _initializationSequence.Length / 2; i++) {
                var register = _initializationSequence[i];
                var data = _initializationSequence[i+1];
                if (register == _TFTLCD_DELAYCMD) {
                    Thread.Sleep(data);
                } else {
                    //writeRegister(register, data);
                    Debug.Print("Register: " + register.ToString()); 
                    Debug.Print("Data: " + data.ToString());
                }
            }
        }

        public void Reset() {
          _reset.Write(false);
          Thread.Sleep(2);
          _reset.Write(true);

          // resync
          //writeData(0);
          //writeData(0);
          //writeData(0);  
          //writeData(0);
        }
    }
}
