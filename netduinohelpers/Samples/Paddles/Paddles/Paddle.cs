using netduino.helpers.Imaging;

namespace Paddles {
    public class Paddle {
        public const int Size = 3;

        public int Y {
            get { return _pixels[0].Y; }
            set {
                for (var i = 0; i < Size; i++) {
                    _pixels[i].Y = value + i;
                }
            }
        }

        private readonly Side _side;
        private readonly GameOfPaddles _game;
        private readonly Composition _world;
        private readonly PlayerMissile _ball;
        private readonly PlayerMissile[] _pixels;

        public Paddle(Side side, GameOfPaddles game) {
            _side = side;
            _game = game;
            _world = game.World;
            _ball = _world["ball"];
            _pixels = new PlayerMissile[Size];
            for(var i = 0; i < Size; i++) {
                _pixels[i] = new PlayerMissile(
                    "paddle" +
                    (_side == Side.Right ? 'R' : 'L') +
                    i,
                    _side == Side.Right ? 7 : 0,
                    i,
                    _world);
            }
            _world.Coinc +=
                (s, a) => {
                    if (_ball.X == 0 && _side == Side.Left) {
                        _game.BallGoingRight = true;
                    }
                    if (_ball.X == 7 && _side == Side.Right) {
                        _game.BallGoingRight = false;
                    }
                };
        }
    }

    public enum Side {
        Left,
        Right
    }
}
