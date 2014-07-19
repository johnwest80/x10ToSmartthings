using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace PIPBoy3000 {
    public class GeigerCounter: IDisposable {
        private readonly GeigerCounterStats _stats;
        private Thread _thread;
        private readonly InterruptPort _geigerCounter;
        private long[] _ticks = new long[4];
        private int _randomNumber;
        private int _ticksCount = 0;
        private bool _flipper = false;

        public int GetRandomNumber() {
            return _randomNumber;
        }

        public GeigerCounter(Cpu.Pin interruptPin, GeigerCounterStats stats) {
            _stats = stats;

            _geigerCounter = new InterruptPort(
                interruptPin,
                true,
                Port.ResistorMode.PullUp,
                Port.InterruptMode.InterruptEdgeLevelLow);

            _geigerCounter.OnInterrupt += PulseCounter;
        }

        public void Start() {
            if (_thread != null) {
                throw new ApplicationException("_thread");
            }
            _thread = new Thread(StatsTracker);
            _thread.Start();
        }

        public void Stop() {
            if (_thread == null) {
                throw new ApplicationException("_thread");
            }
            _geigerCounter.DisableInterrupt();
            _thread.Abort();
            _thread.Join();
        }

        public void Dispose() {
            Stop();
            _thread = null;
        }

        private void StatsTracker() {
            _stats.Reset();
            while (true) {
                _stats.TrackStats();
            }
        }

        private void PulseCounter(uint data1, uint data2, DateTime time) {
            _geigerCounter.DisableInterrupt();      

            _stats.Clicks++;

            if (_ticksCount < _ticks.Length) {
                _ticks[_ticksCount++] = time.Ticks;
            } else {
                _ticksCount = 0;                
                _flipper ^= true;

                var Interval1 = _ticks[1] - _ticks[0];
                var Interval2 = _ticks[3] - _ticks[2];
                    
                if (Interval1 != Interval2) {
                    _randomNumber <<= 1;
                    if (_flipper) {
                        _randomNumber |= (Interval1 < Interval2) ? 0 : 1;
                    } else {
                        _randomNumber |= (Interval1 > Interval2) ? 0 : 1;
                    }
                }
            }
            
            // TO DO: 
            // Remove delay.
            // Use a small capacitor instead (value?) on the pin reading the pulses from the geiger counter.
            Thread.Sleep(14);

            _geigerCounter.EnableInterrupt();
        }
    }
}
