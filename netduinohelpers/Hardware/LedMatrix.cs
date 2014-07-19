using System;
using System.Threading;

namespace netduino.helpers.Hardware {
    /// <summary>
    /// Base class abstracting an 8*8 LED matrix bitmap
    /// </summary>
    public abstract class LedMatrix : IDisposable {
        protected const int MatrixSize = 8;
        protected byte[] MatrixBitmap; // bitmap 
        protected Thread DisplayThread;

        protected LedMatrix() {
            MatrixBitmap = new byte[MatrixSize];
        }

        public LedMatrix Initialize() {
            DisplayThread = new Thread(() => DoDisplay(this));
            DisplayThread.Start();
            return this;
        }

        protected void Clear() {
            for(var i = 0; i < MatrixSize; i++) {
                OnCol(i, 0, false);
                OnRow(i, 0, false);
            }
        }

        /// <summary>
        /// Turn a pixel ON or OFF in the internal 8*8 matrix bitmap representing the display
        /// Not thread safe by design to avoid blocking threads.
        /// </summary>
        /// <param name="row">logical row number</param>
        /// <param name="col">logical column number</param>
        /// <param name="state">true: turn ON the pixel, false: turn OFF the pixel</param>
        public void Set(int row, int col, bool state) {
            // Create a mask to work with the pixel
            byte mask = 1;

            // Shift the bit into the position corresponding to the column
            mask <<= col;

            if (state) // Pixel ON?
            {
                MatrixBitmap[row] |= mask;
            }
            else // Pixel OFF!
            {
                mask ^= 0xff;
                MatrixBitmap[row] &= mask;
            }
        }

        /// <summary>
        /// Set the internal bitmap using another bitmap.
        /// Not thread safe by design to avoid blocking threads.
        /// </summary>
        /// <param name="bitmap">An array of 8 bytes, each representing a row to be displayed in sequence, starting at byte zero in the array</param>
        public void Set(byte[] bitmap) {
            for (var row = 0; row < MatrixSize; row++) {
                MatrixBitmap[row] = bitmap[row];
            }
        }

        /// <summary>
        /// Relies on persistence of vision to display the 8*8 bitmap in memory, one line at a time, one column at a time
        /// Each line is scanned top-bottom and right to left.
        /// The function should be called 30 times per second to avoid flickering.
        /// </summary>
        protected static void DoDisplay(LedMatrix matrix) {
            while (true) {
                var row = 0;

                foreach (var bitmap in matrix.MatrixBitmap) {
                    matrix.OnRow(row, bitmap, true);

                    byte mask = 1;
                    var col = 0;

                    while (mask != 0x80 /* 10000000 */) {
                        var state = bitmap & mask;
                        if (matrix.OnCol(col, bitmap, 1 == state)) break;

                        // Next column
                        mask <<= 1;
                        col++;
                    }

                    // Turn off the current row in the LED matrix before moving on to the next one
                    matrix.OnRow(row, bitmap, false);
                    row++;
                }
            }
        }

        /// <summary>
        /// This method is called for each line in the 8*8 matrix
        /// </summary>
        /// <param name="logicalRow">0-based row number in the matrix</param>
        /// <param name="bitmap">A byte representing the bitmap of the current row</param>
        /// <param name="energize">true: the row should be energized in the physical matrix. false: the row should be turned off in the matrix.</param>
        public virtual void OnRow(int logicalRow, byte bitmap, bool energize) {
            // Override to do actual work with the row
        }

        /// <summary>
        /// This method is called for each column (or bit) in the current row, starting with the LSB.
        /// If the columns need to be accessed all at once, you can use the bitmap parameter do set them all at once. Make sure to return 'true' to indicate that you're done with processing the columns.
        /// Return 'false' if you need to process the columns one at a time.
        /// </summary>
        /// <param name="logicalCol">0-based column number in the matrix</param>
        /// <param name="bitmap">A byte representing the bitmap of the current row</param>
        /// <param name="energize">true: the column should be energized in the physical matrix. false: the column should be turned off in the matrix.</param>
        /// <returns>true: all columns were processed. false: only one column was processed, get the next one.</returns>
        public virtual bool OnCol(int logicalCol, byte bitmap, bool energize) {
            // Override to do actual work with the columns
            return true;
        }

        public void Dispose() {
            Clear();
            if (DisplayThread != null) {
                DisplayThread.Abort();
            }
        }
    }
}