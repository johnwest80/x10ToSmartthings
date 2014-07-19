using Microsoft.SPOT;

namespace PIPBoy3000 {
    public static class Trace {
        public static void Print(string str) {
#if DEBUG
            Debug.Print(str);
#endif
        }
    }
}
