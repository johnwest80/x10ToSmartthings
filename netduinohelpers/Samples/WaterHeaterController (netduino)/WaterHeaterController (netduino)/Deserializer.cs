using System;

namespace WaterHeaterController
{
    public class Deserializer
    {
        private readonly byte[] serializeBuffer;
        private int currentIndex;

        public Deserializer(byte[] buffer) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }

            serializeBuffer = buffer;
            currentIndex = 0;
        }

        public byte Deserialize() {
            if (currentIndex < serializeBuffer.Length) {
                return serializeBuffer[currentIndex++];
            }

            throw new ArgumentOutOfRangeException("currentIndex");
        }
    }
}
