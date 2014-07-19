#define NETDUINO_MINI

using System;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using netduino.helpers.Hardware;
using netduino.helpers.SerialUI;
using netduino.helpers.Servo;
using System.IO.Ports;

#if NETDUINO_MINI
    // You must ensure that you also have the reference set to SecretLabs.NETMF.Hardware.NetduinoMini in the project
    // You must also remove the SecretLabs.NETMF.Hardware.Netduino if it was there.
    using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
    // You must ensure that you also have the reference set to SecretLabs.NETMF.Hardware.Netduino in the project
    // You must also remove the SecretLabs.NETMF.Hardware.NetduinoMini if it was there.
    using SecretLabs.NETMF.Hardware.Netduino;
#endif

namespace WaterHeaterController {
    public class Program {

#if NETDUINO_MINI
        private static readonly OutputPort _servoPowerEnable = new OutputPort(Pins.GPIO_PIN_16, false);
        private static readonly PushButton _pushButton = new PushButton(Pin: Pins.GPIO_PIN_17, Target: PushButtonHandler);
        private static readonly OutputPort _ledOverride = new OutputPort(Pins.GPIO_PIN_13, false);
        private static readonly OutputPort _ledServoPowerEnable = new OutputPort(Pins.GPIO_PIN_15, false); 
        private static readonly PWM _ledLowHeat = new PWM(Pins.GPIO_PIN_18);
        private static readonly PWM _ledHighHeat = new PWM(Pins.GPIO_PIN_19);
        private static readonly HS6635HBServo _servo = new HS6635HBServo(Pins.GPIO_PIN_20,minPulse: 700, centerPulse: 1600);
        private static SerialUserInterface _serialUI = new SerialUserInterface(Serial.COM2);
#else
        private static readonly OutputPort _servoPowerEnable = new OutputPort(Pins.GPIO_PIN_D3, false);
        private static readonly PushButton _pushButton = new PushButton(Pin: Pins.ONBOARD_SW1, Target: PushButtonHandler);
        private static readonly OutputPort _ledOverride = new OutputPort(Pins.GPIO_PIN_D2, false);
        private static readonly OutputPort _ledServoPowerEnable = new OutputPort(Pins.GPIO_PIN_D4, false);
        private static readonly PWM _ledLowHeat = new PWM(Pins.GPIO_PIN_D5);
        private static readonly PWM _ledHighHeat = new PWM(Pins.GPIO_PIN_D6);
        private static readonly HS6635HBServo _servo = new HS6635HBServo(Pins.GPIO_PIN_D9,minPulse: 700, centerPulse: 1600);
        private static SerialUserInterface _serialUI = new SerialUserInterface();
#endif
        private static readonly Schedule _schedule = new Schedule();
        private static readonly Status _status = new Status();
        private static readonly DS1307 _clock = new DS1307();

        private const string Divider = "-------------------------------------------------------------\r\n";

        private static bool _showMainMenu = true;
        private static bool _scheduleChange = true;
        private static bool _shutdown = false;

        private const uint LowHeat = 180;
        private const uint Center = 100;
        private const uint HighHeat = 0;

        public static void Main() {

            const int millisecIncrement = 100;
            const int millisecElapsedLimit = 60000;

            var millisecCounter = 0;
            var currentHeat = HighHeat;

            var currentHeaterStatus = false;
            var previousHeaterStatus = true;
            var initialize = true;

            _ledHighHeat.SetDutyCycle(0);
            _ledLowHeat.SetDutyCycle(0);

            InitializeClock();

            Log("\r\nWater Heater Controller v1.0\r\n"); 
            Log("Initializing...");
            LoadSchedule();

            PowerServo(true);
            Log("Centering servo");
            _servo.Center();
            Log("Setting heater on high heat by default");
            _servo.Move(Center, currentHeat);
            Log("Running...");
            PowerServo(false);

            while (true) {
                if (_serialUI.SerialErrorReceived == true) {
                    _serialUI.Dispose();
                    _serialUI = new SerialUserInterface();
                    _showMainMenu = true;
                    Log("Serial error received: serial UI object recycled");
                }

                if (_showMainMenu == true) {
                    _showMainMenu = false;
                    MainMenu(null);
                }

                millisecCounter += millisecIncrement;

                if (millisecCounter >= millisecElapsedLimit || _scheduleChange == true) {
                    millisecCounter = 0;
                    currentHeaterStatus = _schedule.GetHeaterStatus(_clock.Get());
                }

                if (currentHeaterStatus != previousHeaterStatus || _scheduleChange == true || initialize == true) {
                    Log("Heater state change"); 

                    _ledOverride.Write(_schedule.WaterHeaterManualOverride);

                    if (currentHeaterStatus) {
                        Log("Setting heater on high");
                        PowerServo(true);
                        _servo.Move(currentHeat, HighHeat);
                        PowerServo(false); 
                        _ledHighHeat.SetDutyCycle(50);
                        _ledLowHeat.SetDutyCycle(0);
                        currentHeat = HighHeat;
                    } else {
                        Log("Setting heater on low");
                        PowerServo(true);
                        _servo.Move(currentHeat, LowHeat);
                        PowerServo(false); 
                        _ledHighHeat.SetDutyCycle(0);
                        _ledLowHeat.SetDutyCycle(50);
                        currentHeat = LowHeat;
                    }
                    previousHeaterStatus = currentHeaterStatus;
                    _scheduleChange = false;
                    initialize = false;
                }

                if (_shutdown) {
                    Log("Shutting down");
                    Log("Moving servo to center...");
                    _ledHighHeat.SetDutyCycle(50);
                    _ledLowHeat.SetDutyCycle(50);
                    PowerServo(true);
                    _servo.Move(currentHeat, Center);
                    PowerServo(false);

                    Log("Shutdown complete");
                    Log("Cycle power to restart.");

                    var dutyCycle = 0;
                    var direction = 1;

                    while (true) {
                        _ledHighHeat.SetDutyCycle((uint)dutyCycle);
                        _ledLowHeat.SetDutyCycle((uint)dutyCycle);
                        dutyCycle += direction;
                        if (dutyCycle == 50) {
                            direction = -1;
                        } else if (dutyCycle == 0) {
                            direction = 1;
                        }
                        Thread.Sleep(50);
                    }
                }

                Thread.Sleep(millisecIncrement);
            }
        }

        private static void InitializeClock() {
            try {
                _clock.Get();
            } catch (Exception e) {
                Debug.Print("Initializating the clock with default values due to: " + e);
                byte[] ram = new byte[DS1307.DS1307_RAM_SIZE];
                _clock.Set(new DateTime(2011, 1, 1, 12, 0, 0));
                _clock.Halt(false); 
                _clock.SetRAM(ram);
            }
        }

        private static void PowerServo(bool power) {
            _ledServoPowerEnable.Write(power);
            _servoPowerEnable.Write(power);
        }

        private static void LoadSchedule() {
            Log("Loading schedule");
            try {
                var clockRAM = _clock.GetRAM();
                var deserializer = new Deserializer(clockRAM);
                _schedule.Deserialize(deserializer);
            } catch (Exception e) {
                Log("Failure deserializing schedule from clock RAM. Exception: " + e);
            }
        }

        private static void SaveSchedule() {
            Log("Saving schedule");
            var clockRAM = new byte[DS1307.DS1307_RAM_SIZE];
            var serializer = new Serializer(clockRAM);
            _schedule.Serialize(serializer);
            _clock.SetRAM(clockRAM);
            _scheduleChange = true;
        }

        private static void Log(string line) {
            _serialUI.Display("[" + _clock.Get().ToString() + "] " + line + "\r\n");
        }

        public static void MainMenu(SerialInputItem item) {
            var SystemStatus = new ArrayList();
            _status.Display(SystemStatus, _clock, _schedule);

            _serialUI.Stop();

            _serialUI.AddDisplayItem(Divider); 
            _serialUI.AddDisplayItem(SystemStatus);
            _serialUI.AddDisplayItem("\r\n");
            _serialUI.AddDisplayItem("Main Menu:\r\n");
            _serialUI.AddInputItem(new SerialInputItem { Option = "1", Label = ": Show Schedule", Callback = ShowSchedule });
            _serialUI.AddInputItem(new SerialInputItem { Option = "2", Label = ": Set Schedule", Callback = SetSchedule });
            _serialUI.AddInputItem(new SerialInputItem { Option = "3", Label = ": Set Clock", Callback = SetClock, Context = 0 });
            _serialUI.AddInputItem(new SerialInputItem { Option = "4", Label = ": Swith Heater ON / Resume Schedule", Callback = SwitchHeaterOn });
            _serialUI.AddInputItem(new SerialInputItem { Option = "X", Label = ": Shutdown", Callback = Shutdown });
            _serialUI.AddInputItem(new SerialInputItem { Callback = RefreshMainMenu });

            _serialUI.Go();
        }

        public static void Shutdown(SerialInputItem item) {
            _shutdown = true;
        }

        public static void RefreshMainMenu(SerialInputItem item) {
            _showMainMenu = true;
        }

        public static void ShowSchedule(SerialInputItem item) {
            var scheduleList = new ArrayList();
            _schedule.Display(scheduleList);

            _serialUI.Stop();

            _serialUI.AddDisplayItem(Divider);
            _serialUI.AddDisplayItem(scheduleList);

            _serialUI.Go();

            RefreshMainMenu(null);
        }

        public static void SetSchedule(SerialInputItem item) {
            _serialUI.Stop();

            switch (item.Context) {
                case 0:
                    _serialUI.Store.Clear();
                    _serialUI.AddDisplayItem(Divider);
                    _serialUI.AddDisplayItem("Set Heater Weekly Schedule:\r\n");
                    _serialUI.AddInputItem(new SerialInputItem { Label = GetDayOfWeekSelectionString(), Callback = SetSchedule, Context = 1, StoreKey = "sched.dow" });
                    break;
                case 1:
                    var timeslotList = new ArrayList();
                    var dow = ToInt("sched.dow");
                    if (dow >= 0 && dow <= 6) {
                        _serialUI.AddDisplayItem("Select '" + _schedule.DayList[dow] + "' timeslot\r\n");
                        var slotCount = _schedule.GetDayTimeSlotList((DayOfWeek)dow, timeslotList);
                        slotCount -= 1;
                        _serialUI.Store["sched.maxSlot"] = slotCount.ToString();
                        _serialUI.AddDisplayItem(timeslotList);
                        var slotSelection = "Timeslot (0-" + slotCount.ToString() + ")?";
                        _serialUI.AddInputItem(new SerialInputItem{ Label = slotSelection, Callback = SetSchedule, Context = 2, StoreKey = "sched.timeSlot"} );
                    } else {
                        Log("Invalid week day input.");
                        _serialUI.Store.Clear();
                        RefreshMainMenu(null);
                        return;
                    }
                    break;
                case 2:
                    var maxSlot = ToInt("sched.maxSlot");
                    var timeSlot = ToInt("sched.timeSlot");
                    if (timeSlot >= 0 && timeSlot <= maxSlot) {
                        _serialUI.AddInputItem(new SerialInputItem { Label = "Begin Hour (00-23)?", Callback = SetSchedule, Context = 3, StoreKey = "sched.beginHour" });
                    } else {
                        Log("Invalid timeslot input.");
                        _serialUI.Store.Clear();
                        RefreshMainMenu(null);
                        return;
                    }
                    break;
                case 3:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "End Hour (00-23)?", Callback = SetSchedule, Context = 4, StoreKey = "sched.endHour" });
                    break;
                case 4:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Is this timeslot the same all week (Y/N)?", Callback = SetSchedule, Context = 5, StoreKey = "sched.sameAllWeek" });
                    break;
                case 5:
                    var dayOfWeek = ToInt("sched.dow");
                    var timeSlotIndex = ToInt("sched.timeSlot");
                    var beginHour = ToInt("sched.beginHour");
                    var endHour = ToInt("sched.endHour");
                    var sameAllWeek = (string) _serialUI.Store["sched.sameAllWeek"];

                    var validTimeSlot = true;

                    if ((beginHour >= 0 && beginHour <= 23) && (endHour >= 0 && endHour <= 23)) {
                        if (beginHour > endHour && endHour == 0 || beginHour <= endHour) {
                            validTimeSlot = true;
                        } else {
                            validTimeSlot = false;
                        }
                    } else {
                        validTimeSlot = false;
                    }
                        
                    if (validTimeSlot == false) {
                        Log("Invalid Begin/End hour input.");
                        Log("Hours must be between 0 and 23.");
                        Log("Begin hour must be <= to End hour except if End hour is 0.");
                        _serialUI.Store.Clear();
                        RefreshMainMenu(null);
                        return;
                    }

                    if (sameAllWeek.ToLower() == "y") {
                        _schedule.SetTimeSlot((DayOfWeek) dayOfWeek, timeSlotIndex, beginHour, endHour, true);
                    } else {
                        _schedule.SetTimeSlot((DayOfWeek) dayOfWeek, timeSlotIndex, beginHour, endHour, false);
                    }

                    SaveSchedule();

                    _serialUI.Store.Clear();
                    RefreshMainMenu(null);
                    return;
            }

            _serialUI.Go();
        }

        private static string GetDayOfWeekSelectionString() {
            var count = 0;
            var dayOfWeekSelection = "Day of the week ( ";
            foreach (string day in _schedule.DayList) {
                dayOfWeekSelection += count.ToString() + ":" + day + " ";
                count++;
            }
            dayOfWeekSelection += ")?";
            return dayOfWeekSelection;
        }

        public static void SetClock(SerialInputItem item) {
            _serialUI.Stop();

            switch (item.Context) {
                case 0:
                    _serialUI.Store.Clear();
                    _serialUI.AddDisplayItem(Divider);
                    _serialUI.AddDisplayItem("Set Clock:\r\n");
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Year (YYYY)?", Callback = SetClock, Context = 1, StoreKey = "clock.YYYY" });
                    break;
                case 1:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Month (01-12)?", Callback = SetClock, Context = 2, StoreKey = "clock.MM" });
                    break;
                case 2:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Day (01-31)?", Callback = SetClock, Context = 3, StoreKey = "clock.DD" });
                    break;
                case 3:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Hour (00-23)?", Callback = SetClock, Context = 4, StoreKey = "clock.HH" });
                    break;
                case 4:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Minute (00-59)?", Callback = SetClock, Context = 5, StoreKey = "clock.MN" });
                    break;
                case 5:
                    _serialUI.AddInputItem(new SerialInputItem { Label = "Second (00-59)?", Callback = SetClock, Context = 6, StoreKey = "clock.SS" });
                    break;
                case 6:
                    try {
                        var dt = new DateTime(
                                        ToInt("clock.YYYY"),
                                        ToInt("clock.MM"),
                                        ToInt("clock.DD"),
                                        ToInt("clock.HH"),
                                        ToInt("clock.MN"),
                                        ToInt("clock.SS"));
                        try {
                            _clock.Set(dt);
                            _scheduleChange = true;
                        } catch (Exception e) {
                            Log("Failed to set the clock. Exception: " + e);
                        }

                    } catch (Exception e)
                    {
                        Log("Invalid date/time input exception: " + e);
                    }

                    _serialUI.Store.Clear();

                    RefreshMainMenu(null);
                    return;
            }
            
            _serialUI.Go();
        }

        private static int ToInt(string name) {
            var value = (string) _serialUI.Store[name];
            var Int = -1;
            try {
                Int = Int32.Parse(value);
            } catch (Exception e) {
                Log("Invalid input exception: " + e);
            }
            return Int;
        }

        public static void SwitchHeaterOn(SerialInputItem item) {
            Log("Switching heater state, please wait...");
            _schedule.WaterHeaterManualOverride ^= true;
            _scheduleChange = true;
            RefreshMainMenu(null);
            while (_scheduleChange == true) {
                Thread.Sleep(10);
            }        
        }

        public static void PushButtonHandler(UInt32 port, UInt32 state, DateTime time) {
            _pushButton.Input.DisableInterrupt();
            SwitchHeaterOn(null);
            _pushButton.Input.EnableInterrupt();
        }
    }
}
