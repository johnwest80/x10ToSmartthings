using System;

namespace WaterHeaterController
{
    public class Serializer
    {
        private readonly byte[] _serializeBuffer;
        private int _currentIndex;

        public Serializer(byte[] buffer) {
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }

            _serializeBuffer = buffer;
            _currentIndex = 0;
        }

        public void Serialize(byte data) {
            if (_currentIndex < _serializeBuffer.Length) {
                _serializeBuffer[_currentIndex++] = data;
                return;
            }

            throw new ArgumentOutOfRangeException("currentIndex");
        }
    }
}
