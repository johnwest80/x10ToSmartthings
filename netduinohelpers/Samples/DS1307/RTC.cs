using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Hardware;

namespace RTC
{
    public class Program
    {
        public static void Main()
        {
            var clock = new DS1307();

            // Set the clock to some arbitrary date / time
            clock.Set(new DateTime(2011, 1, 2, 20, 20, 20));

            // Make sure the clock is running
            clock.Halt(false);

            // Test reading RTC clock registers
            Debug.Print("Before Halt: " + clock.Get().ToString());

            // Halt the clock for 3 seconds
            clock.Halt(true);
            Thread.Sleep(3000);

            // Should be the same time as "Before Halt"
            Debug.Print("After Halt for 3 seconds (should be the same time): " + clock.Get().ToString());

            // Resume the clock
            clock.Halt(false);

            // Sleep for another 1.5 second
            Thread.Sleep(1500);

            // Should be just one second later since the clock's oscillator was resumed
            Debug.Print("Resumed Clock (should be time + ~1 sec): " + clock.Get().ToString());

            // Requires an oscilloscope or an interrupt handler on the microcontroller to see the effects
            TestSquareWaves(ref clock);

            // Test writing to arbitrary registers
            Debug.Print("Writing distinct RAM registers (writing the register number to itself)");
            for (byte I = DS1307.DS1307_RAM_START_ADDRESS; I <= DS1307.DS1307_RAM_END_ADDRESS; I++)
            {
                clock.WriteRegister(I, I);
            }

            // Test reading from arbitrary registers
            Debug.Print("Reading distinct RAM registers (the registers and values read should be the same)");
            for (byte I = DS1307.DS1307_RAM_START_ADDRESS; I <= DS1307.DS1307_RAM_END_ADDRESS; I++)
            {
                Debug.Print(I.ToString() + ": " + clock[I].ToString());
            }

            // Test writing to the RAM as a single block
            //-------------01234567890123456789012345678901234567890123456789012345
            string Text = "[There are 56 bytes in the RTC RAM buffer and that's it]";
            Debug.Print("Writing string: " + Text + " (Length=" + Text.Length.ToString() + ") to the RAM as a block.");
            var ram = new byte[DS1307.DS1307_RAM_SIZE];
            // Copy the string to the ram buffer
            for (byte I = 0; I < DS1307.DS1307_RAM_SIZE; I++)
            {
                ram[I] = (byte)Text[I];
            }
            // Write it to the RAM in the clock
            clock.SetRAM(ram);

            // Zero out the ram buffer
            ram = null;
            // Zero out the string
            Text = null;

            // Test reading from the RAM as a single block
            Debug.Print("Reading from the RAM as a block...");
            ram = clock.GetRAM();

            for (byte I = 0; I < DS1307.DS1307_RAM_SIZE; I++)
            {
                Text += (char)ram[I];
            }

            Debug.Print("RAM: " + Text + " (Length=" + Text.Length.ToString() + ")");

            // Sleep another 5 seconds before exiting
            Thread.Sleep(5 * 1000);

            // Reset the clock & RAM
            clock.Set(new DateTime(2011, 2, 17, 21, 36, 00));

            for (byte I = 0; I < DS1307.DS1307_RAM_SIZE; I++) {
                ram[I] = (byte)0;
            }
            // Write it to the RAM in the clock
            clock.SetRAM(ram);            
        }

        // Test the square wave frequencies supported by the clock (oscilloscope or interrupt handler useful here).
        public static void TestSquareWaves(ref DS1307 clock)
        {
            Debug.Print("1Hz frequency test");
            clock.SetSquareWave(DS1307.SQWFreq.SQW_1Hz, DS1307.SQWDisabledOutputControl.One);
            Thread.Sleep(5 * 1000);

            Debug.Print("4kHz frequency test");
            clock.SetSquareWave(DS1307.SQWFreq.SQW_4kHz, DS1307.SQWDisabledOutputControl.One);
            Thread.Sleep(5 * 1000);

            Debug.Print("8kHz frequency test");
            clock.SetSquareWave(DS1307.SQWFreq.SQW_8kHz, DS1307.SQWDisabledOutputControl.One);
            Thread.Sleep(5 * 1000);

            Debug.Print("32kHz frequency test");
            clock.SetSquareWave(DS1307.SQWFreq.SQW_32kHz, DS1307.SQWDisabledOutputControl.One);
            Thread.Sleep(5 * 1000);

            // Test the logic levels when the oscillator is off
            clock.Halt(true);

            // No frequency, square wave output pin pulled high
            Debug.Print("Square Wave disabled, square wave output pin pulled high");
            clock.SetSquareWave(DS1307.SQWFreq.SQW_OFF, DS1307.SQWDisabledOutputControl.One);
            Thread.Sleep(5 * 1000);

            // No frequency, square wave output pin pulled low
            Debug.Print("Square Wave disabled, square wave output pin pulled low");
            clock.SetSquareWave(DS1307.SQWFreq.SQW_OFF, DS1307.SQWDisabledOutputControl.Zero);
            Thread.Sleep(5 * 1000);

            // Resume the oscillator
            clock.Halt(false);
        }
    }
}

