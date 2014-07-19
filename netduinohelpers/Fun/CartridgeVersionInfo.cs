namespace netduino.helpers.Fun {
    public class CartridgeVersionInfo {
        public const int CurrentVersion = 100;

        public enum LoaderArgumentsVersion100 {
            Version,
            JoystickLeft,
            JoystickRight,
            Matrix,
            Speaker,
            SDResourceLoader,
            ButtonLeft,
            ButtonRight,
            Size
        }
    }
}
