using System;
using System.Text;
using System.IO.Ports;
using System.Collections;
using Microsoft.SPOT;

namespace netduino.helpers.SerialUI {
    public delegate void SerialInputResponseHandler(SerialInputItem inputItem);

    /// <summary>
    /// This class encapsulates a set of properties defining user input items
    /// </summary>
    public class SerialInputItem {
        // An option in an list of possible items to choose from.
        // Can be left blank when the Label field is used exclusively.
        public string Option { get; set; }
        // The label to be displayed next to the option
        public string Label { get; set; }
        // Key under which the user input will be stored
        public string StoreKey { get; set; }
        // Context number used to track state when a series of sequential entries are required to complete an input
        public int Context { get; set; }
        // Method to be called back once the input has been received for this option or item in a series of inputs
        public SerialInputResponseHandler Callback { get; set; }

        public SerialInputItem() {
            Option = "";
            Label = "";
            StoreKey = "";
            Context = 0;
            Callback = null;
        }
    }

    /// <summary>
    /// This class enables building simple user interfaces to send and receive data over a serial port.
    /// Incoming serial data is handled in an event-driven manner.
    /// </summary>
    public class SerialUserInterface : IDisposable {
        private SerialPort _comPort;
        private ArrayList _inputItems = new ArrayList();
        private ArrayList _displayList = new ArrayList();
        private byte _comPortReadBufferIndex = 0;
        private byte[] _comPortReadBuffer = new byte[80];
        private bool _processInput = false;
        private UTF8Encoding _encoding = new UTF8Encoding();

        // Default error message to be displayed when an input doesn't match any of the options in SerialInputItem objects
        public string ErrorMessage { get; set; }
        // User-defined dictionary used to store input data by name
        public Hashtable Store = new Hashtable(10);
        // Set to true when a critical error occured with the serial port, indicating that it should be recycled
        public bool SerialErrorReceived { get; private set; }

        public SerialUserInterface(string port = Serial.COM1) {
            SerialErrorReceived = false;
            _comPort = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
            _comPort.ReadTimeout = 20;
            _comPort.WriteTimeout = 20;
            _comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            _comPort.ErrorReceived += new SerialErrorReceivedEventHandler(ErrorReceivedHandler);
            _comPort.Open();

            Stop();
        }

        /// <summary>
        /// Stop accepting input from the serial port
        /// </summary>
        public void Stop() {
            _processInput = false;
            _inputItems.Clear();
            _displayList.Clear();
            ErrorMessage = "Invalid input.\r\n";
        }

        // Add an input item to the user interface
        public void AddInputItem(SerialInputItem item) {
            _inputItems.Add(item);
        }

        // Add an display item to the user interface
        public void AddDisplayItem(string item) {
            _displayList.Add(item);
        }

        // Add a list of display items to the user interface
        public void AddDisplayItem(ArrayList list) {
            foreach (string line in list) {
                _displayList.Add(line);
            }
        }

        // Immediately send a list of strings to the serial port
        public void Display(ArrayList list) {
            foreach (string line in list) {
                Display(line);
            }
            _comPort.Flush();
        }

        // Immediately send a line of text to the serial port
        public void Display(string line) {
            byte[] bytes = _encoding.GetBytes(line);
            _comPort.Write(bytes, 0, bytes.Length);
            //Debug.Print(line);
        }

        // Start accepting user input from the serial port
        public void Go() {
            Display(_displayList);

            var inputList = new ArrayList();

            foreach (SerialInputItem item in _inputItems) {
                inputList.Add("\t" + item.Option + " " + item.Label + "\r\n");
            }

            Display(inputList);

            _comPort.DiscardInBuffer();
            _comPortReadBufferIndex = 0;
            _comPortReadBuffer[_comPortReadBufferIndex] = 0;
            _processInput = true;
        }

        // Free up the resources
        public void Dispose() {
            _comPort.Flush();
            _comPort.Close();
            _comPort = null;
            _comPortReadBuffer = null;
            _inputItems = null;
            _displayList = null;
            _encoding = null;
            Store = null;
        }

        // Interrupt-driven serial data handler.
        // Accepts up to 80 characters.
        // Invokes user-callbacks when a carriage-return or a Line-feed is detected.
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
            var sp = (SerialPort)sender;

            if (_processInput == true) {
                var bytes = sp.BytesToRead;
                while (bytes != 0) {
                    bytes--;
                    if (_comPortReadBufferIndex < _comPortReadBuffer.Length) {
                        sp.Read(_comPortReadBuffer, _comPortReadBufferIndex, 1);
                        sp.Write(_comPortReadBuffer, _comPortReadBufferIndex, 1);
                        if (_comPortReadBuffer[_comPortReadBufferIndex] == '\r' || 
                            _comPortReadBuffer[_comPortReadBufferIndex] == '\n') {
                            Display("\r\n");
                            _comPortReadBuffer[_comPortReadBufferIndex] = 0;
                            sp.DiscardInBuffer();
                            var found = false;
                            var inputData = MakeLowerString(_comPortReadBuffer);
                            foreach (SerialInputItem item in _inputItems) {
                                if (item.Option.ToLower() == inputData || item.Option.Length == 0) {
                                    if (item.StoreKey != null && item.StoreKey.Length != 0) {
                                        Store[item.StoreKey] = inputData;
                                    }
                                    item.Callback(item);
                                    found = true;
                                    break;
                                }
                            }
                            if (found == false) {
                                Display(ErrorMessage);
                            }
                            break;
                        }
                    } else {
                        sp.DiscardInBuffer();
                        _comPortReadBufferIndex = 0;
                        _comPortReadBuffer[_comPortReadBufferIndex] = 0;
                        break;
                    }
                    _comPortReadBufferIndex++;
                }
            } else {
                sp.DiscardInBuffer();
            }
        }

        private string MakeLowerString(byte[] data) {
            var str = new string(Encoding.UTF8.GetChars(data));
            if (str != null) {
                return str.ToLower();
            } else {
                return "";
            }
        }

        private void ErrorReceivedHandler(object sender, SerialErrorReceivedEventArgs e) {
            SerialErrorReceived = true;
        }
    }
}
