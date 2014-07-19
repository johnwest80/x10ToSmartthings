using System;
using netduino.helpers.Imaging;
using netduino.helpers.Math;

namespace Meteors {
    public class Meteor {
        public const float MeteorSpeed = 0.1f;

        private readonly byte[] _rockOffsets =
            new byte[] {
                           0, 0,
                           1, 0,
                           1, 1,
                           0, 1
                       };
        private readonly PlayerMissile[] _rocks = new PlayerMissile[3];
        private readonly byte[] _rockXOffsets = new byte[3];
        private readonly byte[] _rockYOffsets = new byte[3];
        private readonly Random _rnd;

        public bool IsExploded { get; set; }

        public Meteor(GameOfMeteors game, int index, int x, int y) {
            _rnd = new Random();
            var speed = GetRandomSpeed();
            var j = 0;
            var skip = _rnd.Next(4);
            for (var i = 0; i < 4; i++) {
                if (i == skip) continue;
                _rockXOffsets[j] = _rockOffsets[i*2];
                _rockYOffsets[j] = _rockOffsets[i*2 + 1];
                _rocks[j] = new PlayerMissile {
                                                  Name = "Meteor" + index + ":" + j,
                                                  X = x + _rockXOffsets[j],
                                                  Y = y + _rockYOffsets[j],
                                                  HorizontalSpeed = speed.X,
                                                  VerticalSpeed = speed.Y,
                                                  Owner = game.World
                                              };
                j++;
            }
        }

        private Vector2D GetRandomSpeed() {
            var dir = (float)_rnd.NextDouble() * 2 * Trigo.Pi;
            return new Vector2D {
                                    X = Trigo.Cos(dir)*MeteorSpeed,
                                    Y = Trigo.Sin(dir)*MeteorSpeed
                                };
        }

        public void Move() {
            for(var i = 0; i < _rocks.Length; i++) {
                if (!_rocks[i].IsVisible) continue;
                _rocks[i].Move();
                GameOfMeteors.ApplyToreGeometry(_rocks[i]);
            }
        }

        public bool Has(PlayerMissile someRock) {
            foreach (var rock in _rocks) {
                if (rock == someRock && rock.IsVisible) return true;
            }
            return false;
        }

        public void Explode() {
            _rocks[1].IsVisible = false;
            var speed = GetRandomSpeed();
            _rocks[0].HorizontalSpeed = speed.X;
            _rocks[0].VerticalSpeed = speed.Y;
            speed = GetRandomSpeed();
            _rocks[2].HorizontalSpeed = speed.X;
            _rocks[2].VerticalSpeed = speed.Y;
            IsExploded = true;
        }
    }
}
