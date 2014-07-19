using System.Threading;
using netduino.helpers.Fun;
using netduino.helpers.Hardware;
using netduino.helpers.Imaging;

namespace Meteors {
    public class GameOfMeteors : Game {
        public const int WorldSize = 8;
        public const int NumberOfMeteors = 1;
        private const float EnginePower = 0.3f;
        private const float PruneauSpeed = 1f;

        public Meteor[] Meteors { get; private set; }
        public PlayerMissile Pruneau { get; private set; }
        public PlayerMissile Ship { get; private set; }

        public GameOfMeteors(ConsoleHardwareConfig config)
            : base(config) {
            World = new Composition(new byte[WorldSize * WorldSize / 8], WorldSize, WorldSize);
            World.Coinc += WorldCoinc;
            Ship = new PlayerMissile("ship", WorldSize / 2, WorldSize / 2, World);
            Ship.X = WorldSize / 2;
            Ship.Y = WorldSize / 2;
            Pruneau = new PlayerMissile {
                Name = "Pruneau",
                Owner = World,
                IsVisible = false
            };
            Meteors = new Meteor[NumberOfMeteors];
            for (var i = 0; i < NumberOfMeteors; i++) {
                Meteors[i] = new Meteor(this, i,
                    new[] {0, WorldSize - 2, 0, WorldSize - 2} [i],
                    new[] {0, 0, WorldSize - 2, WorldSize - 2} [i]);
            }
            DisplayDelay = 0;
        }

        void WorldCoinc(object sender, CoincEventArgs e) {
            if (e.Missile1 == Pruneau || e.Missile2 == Pruneau) {
                // Explode rock or meteor
                var rock = e.Missile1 == Pruneau ? e.Missile2 : e.Missile1;
                // Does this rock belong to a meteor?
                foreach (var meteor in Meteors) {
                    if (meteor.Has(rock)) {
                        if (meteor.IsExploded) {
                            rock.IsVisible = false;
                        }
                        else {
                            meteor.Explode();
                            MakeExplosionSound();
                        }
                        Pruneau.IsVisible = false;
                        return;
                    }
                }
            }
            if (e.Missile1 == Ship || e.Missile2 == Ship) {
                // You lose
            }
        }

        public void MakeExplosionSound() {
            for (uint frequency = 6000; frequency < 1000; frequency -= 500) {
                Beep(frequency, 20);
            }
        }

        public override void Loop() {
            // Ship
            Ship.HorizontalSpeed = (float)Hardware.JoystickLeft.XDirection * EnginePower;
            Ship.VerticalSpeed = (float)Hardware.JoystickLeft.YDirection * EnginePower;
            Ship.Move();

            var ship = Ship;
            ApplyToreGeometry(ship);

            // Meteors
            foreach (var meteor in Meteors) {
                meteor.Move();
            }

            // Pruneau
            if (Pruneau.IsVisible) {
                Pruneau.Move();
                if (Pruneau.ExactX < 0 ||
                    Pruneau.ExactY < 0 ||
                    Pruneau.ExactX >= WorldSize ||
                    Pruneau.ExactY >= WorldSize) {

                    Pruneau.IsVisible = false;
                }
            }
            else {
                var shootXDir = Hardware.JoystickRight.XDirection;
                var shootYDir = Hardware.JoystickRight.YDirection;
                if (shootXDir != AnalogJoystick.Direction.Center ||
                    shootYDir != AnalogJoystick.Direction.Center) {

                    Pruneau.ExactX = Ship.ExactX;
                    Pruneau.ExactY = Ship.ExactY;
                    Pruneau.HorizontalSpeed = (float) shootXDir*PruneauSpeed;
                    Pruneau.VerticalSpeed = (float) shootYDir*PruneauSpeed;
                    Pruneau.IsVisible = true;
                    Beep(2000, 20);
                    Beep(1000, 20);
                }
            }

            // Display
            Hardware.Matrix.Display(World.GetFrame(0, 0));
        }

        public static void ApplyToreGeometry(PlayerMissile missile) {
            if (missile.ExactX < 0) missile.ExactX += WorldSize;
            if (missile.ExactY < 0) missile.ExactY += WorldSize;
            if (missile.ExactX >= WorldSize) missile.ExactX -= WorldSize;
            if (missile.ExactY >= WorldSize) missile.ExactY -= WorldSize;
        }

        public void Beep(uint frequency, int delayms) {
            var period = 1000000 / frequency;
            Hardware.Speaker.SetPulse(period, period / 2);
            Thread.Sleep(delayms);
            Hardware.Speaker.SetPulse(0, 0);
        }
    }
}
