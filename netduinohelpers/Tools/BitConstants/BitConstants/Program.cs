using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitConstants {
    class Program {
        static void Main(string[] args) {
            for (var i = 0; i < 256; i++) {
                Console.Write("        public const byte ");
                for (var bit = 7; bit >= 0; bit--) {
                    Console.Write(((i >> bit) & 0x01) == 1 ? 'X' : '_');
                }
                Console.Write(" = ");
                Console.Write(i);
                Console.WriteLine(';');
            }
        }
    }
}
