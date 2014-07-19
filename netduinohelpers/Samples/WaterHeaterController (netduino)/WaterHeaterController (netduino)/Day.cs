using System;
using Microsoft.SPOT;
using System.Collections;

namespace WaterHeaterController {
    public class Day : ISerializable {
        public ArrayList timeSlots { get; private set; }

        public Day() {
            timeSlots = new ArrayList();
            for (var slot = 0; slot < 4; slot++) {
                timeSlots.Add(new TimeSlot());
            }
        }

        public void Serialize(Serializer serial) {
            foreach (TimeSlot slot in timeSlots) {
                slot.Serialize(serial);
            }
        }

        public void Deserialize(Deserializer serial) {
            foreach (TimeSlot slot in timeSlots) {
                slot.Deserialize(serial);
            }
        }

        public void Display(ArrayList displayList) {
            foreach (TimeSlot slot in timeSlots) {
                displayList.Add(" ");
                slot.Display(displayList);
            }
        }

        public int GetTimeSlotList(ArrayList displayList) {
            var count = 0;
            foreach (TimeSlot slot in timeSlots) {
                displayList.Add(count.ToString() + " : ");
                slot.Display(displayList);
                displayList.Add("\r\n");
                count++;
            }
            return count;
        }

        public bool CheckTimeSlots(DateTime now) {
            foreach (TimeSlot slot in timeSlots) {
                if (slot.CompareTo(now) == true) {
                    return true;
                }
            }
            return false;
        }
    }
}
