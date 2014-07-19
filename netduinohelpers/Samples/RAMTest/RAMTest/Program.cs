using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RAMTest {
    public class Program {
        public static void Main() {
            Debug.EnableGCMessages(true);
            Debug.Print(Debug.GC(true).ToString());
        }
    }
}
