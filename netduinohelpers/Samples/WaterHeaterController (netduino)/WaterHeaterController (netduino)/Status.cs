using System;
using System.Collections;
using netduino.helpers.Hardware;

namespace WaterHeaterController {
    public class Status {
        public void Display(ArrayList displayList, DS1307 clock, Schedule schedule) {
            var now = clock.Get();

            displayList.Add("Time: " + now.ToString("U") + "\r\n");
            displayList.Add("Heater Schedule Today: ");
            schedule.GetDayTimeSlots(now.DayOfWeek, displayList);
            displayList.Add("\r\n");
            displayList.Add("Heater Status: ");

            if (schedule.GetHeaterStatus(now) == true) {
                displayList.Add("ON");
            } else {
                displayList.Add("OFF");
            }

            if (schedule.WaterHeaterManualOverride) {
                displayList.Add(" [manual override]");
            } else {
                displayList.Add(" [scheduled]");
            }

            displayList.Add("\r\n");
        }
    }
}
