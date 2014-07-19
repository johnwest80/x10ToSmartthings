using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace _24BitBmpToBigEndianBinary {
    class Program {
        static void Main(string[] args) {
            if (args.Length != 1) {
                Console.WriteLine("Please provide a filename for the bitmap to convert.");
                return;
            }

            using (var bmp = new Bitmap(args[0])) {
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                    Console.WriteLine("Please provide a 24-bit depth bitmap to convert.");
                    return;
                }

                FileStream bin = new FileStream((string)(args[0] + ".24.bin"), FileMode.Create, FileAccess.Write);

                for (int row = 0; row < bmp.Height; row++) {
                    for (int column = 0; column < bmp.Width; column++) {
                        var pixel = bmp.GetPixel(column, row);
                        
                        // Convert from 888 to 565 format
                        ushort pixelOut = (byte) (pixel.R >> 3);
                        pixelOut <<= 6;
                        pixelOut |= (byte) (pixel.G >> 2);
                        pixelOut <<= 5;
                        pixelOut |= (byte) (pixel.B >> 3);

                        bin.WriteByte((byte) (pixelOut >> 8));
                        bin.WriteByte((byte) pixelOut);
                    }
                }
                bin.Close();
                Console.WriteLine("Done.");
            }
        }
    }
}
