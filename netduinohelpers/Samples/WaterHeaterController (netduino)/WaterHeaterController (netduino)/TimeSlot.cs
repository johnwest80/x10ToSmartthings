using System;
using Microsoft.SPOT;
using System.Collections;

namespace WaterHeaterController {
    public class TimeSlot : ISerializable {
        public byte BeginHour { get; set; }
        public byte EndHour { get; set; }

        public TimeSlot(byte beginHour = (byte)0, byte endHour = (byte)0) {
            BeginHour = beginHour;
            EndHour = endHour;
        }

        public void Serialize(Serializer serial) {
            serial.Serialize(BeginHour);
            serial.Serialize(EndHour);
        }

        public void Deserialize(Deserializer serial) {
            BeginHour = serial.Deserialize();
            EndHour = serial.Deserialize();
        }

        public void Display(ArrayList displayList) {
            displayList.Add("[" + BeginHour.ToString() + "-" + EndHour.ToString() + "]");
        }

        public bool CompareTo(DateTime now) {
            if (BeginHour != 0 || EndHour != 0) {
                var tempEndHour = EndHour;

                if (tempEndHour == 0) {
                    tempEndHour = 24;
                }

                if ((int)now.Hour >= BeginHour && (int)now.Hour < tempEndHour) {
                    return true;
                }
            }

            return false;
        }
    }
}
