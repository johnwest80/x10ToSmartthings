using System.Threading;

namespace PIPBoy3000 {
    
    public enum StatsChangeEvent {
        ClicksPerTimeSlice,
        ClicksPerMinute,
        ClicksPerHourAverage
    }

    public delegate void StatsChange(StatsChangeEvent changeType, int data, int minuteCounter);

    public class GeigerCounterStats {
        public int WaitTimeSliceInSeconds { get; set; }
        public int MaxWaitTimeSlicesPerMinute { get; set; }

        public int TimeSliceCounter { get; set; }
        public int ClicksPerMinute { get; set; }
        public int MinuteCounter { get; set; }

        public const int Minutes = 60;
        private readonly int[] _clicksInLastHour = new int[Minutes];

        private readonly StatsChange _statsChangeHandler;

        public int Clicks {get; set;}

        public GeigerCounterStats(StatsChange statsChangeHandler) {
            WaitTimeSliceInSeconds = 10;
            MaxWaitTimeSlicesPerMinute = 60 / WaitTimeSliceInSeconds;
            _statsChangeHandler = statsChangeHandler;
            Reset();
        }

        public void Reset() {
            for (uint I = 0; I < _clicksInLastHour.Length; I++) {
                _clicksInLastHour[I] = int.MinValue;
            }
            Clicks = 0;
            TimeSliceCounter = 0;
            ClicksPerMinute = 0;
            MinuteCounter = 0;
        }

        public void TrackStats() {
            Thread.Sleep(WaitTimeSliceInSeconds * 1000);

            ClicksPerMinute += Clicks;
            _statsChangeHandler(StatsChangeEvent.ClicksPerTimeSlice, Clicks, MinuteCounter);
            Clicks = 0;

            TimeSliceCounter++;
            if (TimeSliceCounter == MaxWaitTimeSlicesPerMinute) {
                TimeSliceCounter = 0;

                _statsChangeHandler(StatsChangeEvent.ClicksPerMinute, ClicksPerMinute, MinuteCounter);
                _clicksInLastHour[MinuteCounter++] = ClicksPerMinute;
                ClicksPerMinute = 0;

                if (MinuteCounter == _clicksInLastHour.Length) {
                    MinuteCounter = 0;
                }
                _statsChangeHandler(StatsChangeEvent.ClicksPerHourAverage, GetClicksPerHourAverage(), MinuteCounter);
            }
        }

        public int GetClicksPerHourAverage() {
            var divisor = 0;
            var total = 0;
            for (uint I = 0; I < _clicksInLastHour.Length; I++) {
                if (_clicksInLastHour[I] != int.MinValue) {
                    divisor++;
                    total += _clicksInLastHour[I];
                }
            }
            if (divisor != 0) {
                return total / divisor;
            }
            return 0;
        }
    }
}
