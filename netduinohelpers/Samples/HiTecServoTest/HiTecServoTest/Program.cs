using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Servo;

namespace HiTecServoTest {
    public class Program {
        public static void Main() {
            using (var servo = new HS6635HBServo(Pins.GPIO_PIN_D9))
            {
                servo.Center();
                servo.Move(90, 0, 25);

                while (true)
                {
                    servo.Move(0, 180, 25);
                    servo.Move(180, 0, 25);
                }
            }
        }
    }
}
