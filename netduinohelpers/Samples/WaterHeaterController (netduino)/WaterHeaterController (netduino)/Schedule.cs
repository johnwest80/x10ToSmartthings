using System;
using System.Collections;

namespace WaterHeaterController {
    public class Schedule : ISerializable {
        public ArrayList WeekDays { get; private set; }
        public readonly string[] DayList = new string[] {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};

        public bool WaterHeaterManualOverride { get; set; }

        public Schedule() {
            WaterHeaterManualOverride = false;
            WeekDays = new ArrayList();
            for (var day = 0; day < 7; day++) {
                WeekDays.Add(new Day());
            }
        }

        public void Serialize(Serializer serial) {
            foreach (Day day in WeekDays) {
                day.Serialize(serial);
            }
        }

        public void Deserialize(Deserializer serial) {
            foreach (Day day in WeekDays) {
                day.Deserialize(serial);
            }
        }

        public void Display(ArrayList displayList) {
            displayList.Add("Heater Weekly Schedule:\r\n\r\n");
            var count = 0;
            foreach (Day day in WeekDays) {
                displayList.Add(DayList[count++]);
                day.Display(displayList);
                displayList.Add("\r\n");
            }
        }

        public void GetDayTimeSlots(DayOfWeek dow, ArrayList displayList) {
            var day = (Day) WeekDays[(int) dow];
            displayList.Add(DayList[(int) dow]);
            day.Display(displayList);
        }

        public int GetDayTimeSlotList(DayOfWeek dow, ArrayList displayList) {
            var day = (Day)WeekDays[(int) dow];
            return day.GetTimeSlotList(displayList);
        }

        public bool GetHeaterStatus(DateTime now) {
            if (WaterHeaterManualOverride == true) {
                return true;
            }
            var day = (Day)WeekDays[(int)now.DayOfWeek];
            return day.CheckTimeSlots(now);
        }

        public void SetTimeSlot(DayOfWeek dow, int timeSlotIndex, int beginHour, int endHour, bool sameAllWeek) {
            if (sameAllWeek == true) {
                foreach (Day day in WeekDays) {
                    SetSlot(day, timeSlotIndex, beginHour, endHour);
                }
            } else {
                SetSlot((Day)WeekDays[(int)dow], timeSlotIndex, beginHour, endHour);
            }
        }

        private void SetSlot(Day day, int timeSlotIndex, int beginHour, int endHour) {
            var timeSlotObj = (TimeSlot)day.timeSlots[timeSlotIndex];
            timeSlotObj.BeginHour = (byte)beginHour;
            timeSlotObj.EndHour = (byte)endHour;
        }
    }
}
