using Microsoft.SPOT;

namespace netduino.helpers.Imaging {
    public class CoincEventArgs : EventArgs {
        public CoincEventArgs(PlayerMissile missile1, PlayerMissile missile2) {
            Missile1 = missile1;
            Missile2 = missile2;
        }

        public PlayerMissile Missile1 { get; private set; }
        public PlayerMissile Missile2 { get; private set; }
    }
}
