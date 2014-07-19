namespace netduino.helpers.Imaging {
    public class PlayerMissile {
        public float ExactX;
        public float ExactY;
        private Composition _owner;

        public PlayerMissile() : this(null, 0, 0) {}

        public PlayerMissile(string name, int exactX, int exactY, Composition owner = null) {
            Name = name;
            ExactX = exactX;
            ExactY = exactY;
            HorizontalSpeed = 0;
            VerticalSpeed = 0;
            IsVisible = true;
            Owner = owner;
        }

        public bool IsVisible { get; set; }
        public string Name { get; set; }

        public int X {
            get { return (int)ExactX; }
            set {
                ExactX = value;
                if (Owner != null) {
                    Owner.NotifyChange();
                }
            }
        }
        
        public int Y {
            get { return (int)ExactY; }
            set {
                ExactY = value;
                 if (Owner != null) {
                    Owner.NotifyChange();
                }
           }
        }

        public float HorizontalSpeed { get; set; }
        
        public float VerticalSpeed { get;set; }

        public Composition Owner {
            get {
                return _owner;
            }
            set {
                _owner = value;
                if (_owner != null) {
                    _owner.AddMissile(this);
                }
            }
        }

        public void Move() {
            ExactX += HorizontalSpeed;
            ExactY += VerticalSpeed;
            if (Owner != null) {
                Owner.NotifyChange();
            }
        }
    }
}
