using System;
using netduino.helpers.Fun;
using netduino.helpers.Imaging;
using System.Threading;

namespace Paddles {
    public class GameOfPaddles : Game {
        private const int ScreenSize = 8;
        private const int StickActiveZoneSize = 300;
        private const int StickRange = 1024;
        private const uint BeepFrequency = 10000;
        private const uint BoopFrequency = 3000;
        private const int PaddleAmplitude = ScreenSize - Paddle.Size;
        private const int StickActiveAmplitude = StickActiveZoneSize - Paddle.Size*StickActiveZoneSize/ScreenSize;
        private const int StickMin = (StickRange - StickActiveAmplitude)/2;
        private const int StickMax = (StickRange + StickActiveAmplitude)/2;
        private const int MaxScore = 9;
        private bool _leftButtonClicked;
        private bool _rightButtonClicked;

        bool _ballGoingDown;

        public int LeftScore { get; set; }
        public int RightScore { get; set; }

        public bool BallGoingRight { get; set; }

        public PlayerMissile Ball { get; private set; }
        public Paddle LeftPaddle { get; private set; }
        public Paddle RightPaddle { get; private set; }

        public GameOfPaddles(ConsoleHardwareConfig config) : base(config) {
            Hardware.LeftButton.Input.DisableInterrupt();
            Hardware.RightButton.Input.DisableInterrupt();
            Hardware.LeftButton.Input.OnInterrupt += OnLeftButtonClick;
            Hardware.RightButton.Input.OnInterrupt += OnRightButtonClick;
            Hardware.LeftButton.Input.EnableInterrupt();
            Hardware.RightButton.Input.EnableInterrupt();

            DisplaySplashScreen();

            World = new Composition(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, ScreenSize, ScreenSize);
            Ball = new PlayerMissile("ball", 0, 0, World);
            LeftPaddle = new Paddle(Side.Left, this);
            RightPaddle = new Paddle(Side.Right, this);
            
            ResetBall(true);
        }

        public void DisplaySplashScreen() {
            var charSet = new CharSet(); 
            var splashScreen = charSet.StringToBitmap("  2 paddles and a ball");

            _leftButtonClicked = false;
            _rightButtonClicked = false;

            while (!(_leftButtonClicked || _rightButtonClicked)) {
                var x = 0;
                for (; x < splashScreen.Width; x++) {
                    Hardware.Matrix.Display(splashScreen.GetFrame(x, 0));
                    if (_leftButtonClicked || _rightButtonClicked) {
                        break;
                    }
                    Thread.Sleep(50);
                }
                for (; x != 0; x--) {
                    if (_leftButtonClicked || _rightButtonClicked) {
                        break;
                    }
                    Hardware.Matrix.Display(splashScreen.GetFrame(x, 0));
                    Thread.Sleep(50);
                }
            }
        }

        public override void Loop() {
            var effectiveLeftPaddleY = Hardware.JoystickLeft.Y < StickMin
                                           ? StickMin
                                           : Hardware.JoystickLeft.Y > StickMax
                                                 ? StickMax
                                                 : Hardware.JoystickLeft.Y;
            LeftPaddle.Y = (effectiveLeftPaddleY - StickMin) * PaddleAmplitude / StickActiveAmplitude;
            var effectiveRightPaddleY = Hardware.JoystickRight.Y < StickMin
                                           ? StickMin
                                           : Hardware.JoystickRight.Y > StickMax
                                                 ? StickMax
                                                 : Hardware.JoystickRight.Y;
            RightPaddle.Y = (effectiveRightPaddleY - StickMin) * PaddleAmplitude / StickActiveAmplitude;

            Ball.X += BallGoingRight ? 1 : -1;
            if (Ball.X < 0) {
                RightScore++;
                DisplayScores(LeftScore, RightScore);
                ResetBall(true);
            }
            if (Ball.X >= 8) {
                LeftScore++;
                DisplayScores(LeftScore, RightScore);
                ResetBall(false);
            }
            Ball.Y += _ballGoingDown ? 1 : -1;
            if (Ball.Y < 0) {
                Ball.Y = 1;
                _ballGoingDown = true;
                Beep(BeepFrequency);
            }
            if (Ball.Y >= 8) {
                Ball.Y = 7;
                _ballGoingDown = false;
                Beep(BeepFrequency);
            }
            Hardware.Matrix.Display(World.GetFrame(0, 0));
        }

        private void DisplayScores(int leftScore, int rightScore) {
            Hardware.Matrix.Display(SmallChars.ToBitmap(leftScore, rightScore));
            if (leftScore >= MaxScore || rightScore >= MaxScore) {
                WaitForClick();
                LeftScore = 0;
                RightScore = 0;
            }
            else {
                Thread.Sleep(2000);
            }
        }

        private void WaitForClick() {
            _leftButtonClicked = false;
            _rightButtonClicked = false;

            while (!(_leftButtonClicked || _rightButtonClicked)) {
                Thread.Sleep(100);
            }
        }

        private void OnLeftButtonClick(UInt32 port, UInt32 state, DateTime time) {
            _leftButtonClicked = true;
        }

        private void OnRightButtonClick(UInt32 port, UInt32 state, DateTime time) {
            _rightButtonClicked = true;
        }

        public void ResetBall(bool ballGoingRight) {
            BallGoingRight = ballGoingRight;
            Ball.X = BallGoingRight ? 0 : 7;
            Ball.Y = Random.Next(8);
            _ballGoingDown = Random.Next(2) == 0;
            Beep(BoopFrequency);
        }

        public void Beep(uint frequency) {
            var period = 1000000 / frequency; 
            Hardware.Speaker.SetPulse(period, period / 2);
            Thread.Sleep(50);
            Hardware.Speaker.SetPulse(0, 0);
        }
    }
}
