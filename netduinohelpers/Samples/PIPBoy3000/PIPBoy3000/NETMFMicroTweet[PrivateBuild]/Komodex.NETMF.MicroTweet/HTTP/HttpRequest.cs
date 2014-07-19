using System;
using Microsoft.SPOT;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Komodex.NETMF.MicroTweet.HTTP
{
    public class HttpRequest
    {
        public HttpRequest(string uri)
            : this(new HttpUri(uri))
        { }

        public HttpRequest(HttpUri uri)
        {
            Uri = uri;
            Method = "GET";
            Headers = new HeaderCollection();
            Timeout = 30;
        }

        #region Properties

        public HttpUri Uri { get; set; }

        public string Method { get; set; }

        public HeaderCollection Headers { get; set; }

        public int Timeout { get; set; }
        
        public string ContentType { get; set; }
        
        public string PostContent { get; set; }
        
        public int ContentLength
        {
            get
            {
                if (PostContent == null)
                    return 0;
                return Encoding.UTF8.GetBytes(PostContent).Length;
            }
        }

        #endregion

        #region Public Methods

        public string GetResponse(bool getResponse = true)
        {
            // Build the request
            string request = Method + " " + Uri.Path + " HTTP/1.1\r\n";
            request += "Host: " + Uri.Hostname + "\r\n";

            // Headers
            request += "Connection: Close\r\n";
            if (ContentType != null && ContentType != string.Empty)
                request += "Content-Type: " + ContentType + "\r\n";
            request += "Content-Length: " + ContentLength + "\r\n";
            request += Headers.ToString() + "\r\n\r\n";

            // POST content
            if (PostContent != null && PostContent != string.Empty)
                request += PostContent + "\r\n\r\n";

            // Get the server's IP address
            IPHostEntry hostEntry = Dns.GetHostEntry(Uri.Hostname);

            // Create socket and connect
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Connect(new IPEndPoint(hostEntry.AddressList[0], Uri.Port));

                // Convert the HTTP request to a byte array
                byte[] requestBytes = Encoding.UTF8.GetBytes(request);
                request = null;

                Trace.Print("Sending request");

                // Send the request
                socket.Send(requestBytes);
                requestBytes = null;

                Trace.Print("Waiting specified timeout");

                // Wait for the specified timeout
                DateTime timeoutAt = DateTime.Now.AddSeconds(Timeout);
                while (socket.Available == 0 && DateTime.Now < timeoutAt)
                    Thread.Sleep(100);

                // Set up the result buffers
                byte[] buffer = new byte[256];

                if (getResponse) {
                    string result = string.Empty;
                    // Read the data
                    while (socket.Poll(30 * 1000 * 1000, SelectMode.SelectRead)) {
                        if (socket.Available == 0)
                            break;

                        // Clear the buffer and receive data
                        Array.Clear(buffer, 0, buffer.Length);
                        socket.Receive(buffer);

                        // Append the buffer contents to the result
                        Debug.GC(true);
                        result += new string(Encoding.UTF8.GetChars(buffer));
                    }
                    return result.ToString();
                } else {
                    Trace.Print("Start response dump");
                    while (socket.Poll(30 * 1000 * 1000, SelectMode.SelectRead)) {
                        if (socket.Available == 0)
                            break;
                        socket.Receive(buffer);
                    }
                    Trace.Print("End response dump");
                    return null;
                }
            }
        }

        #endregion

    }
}
