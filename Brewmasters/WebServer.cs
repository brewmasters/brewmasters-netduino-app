using System;
using Microsoft.SPOT;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;
using System.IO;
using Microsoft.SPOT.Net;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Brewmasters
{
    class WebServer : IDisposable
    {
        private const int Backlog = 10;
        private Socket _socket = null;
        private Thread _thread = null;
        private string _location = null;
        private const int FileBufferLength = 1024;
        private const string ResponseBegin = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: ";
        private const string ResponseEnd = "\r\nConnection: close\r\n\r\n";
        private static byte[] ErrorResponse = Encoding.UTF8.GetBytes("<HTML><HEAD><TITLE>Website</TITLE></HEAD><BODY><H1>Content Not Found</H1></BODY></HTML>");
        private static byte[] HelloWorld = Encoding.UTF8.GetBytes("<HTML><HEAD><TITLE>Website</TITLE></HEAD><BODY><H1>Hello World</H1></BODY></HTML>");
        private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);



        public WebServer(string location, int port = 80)
        {
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface NI = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];

            //// DHCP
            //NI.EnableDhcp();
            //NI.ReleaseDhcpLease();
            //NI.RenewDhcpLease();
            //Debug.Print(NI.IPAddress.ToString());

            //// Static
            //NI.EnableStaticIP("192.168.2.75", "255.255.255.0", "192.168.2.1");
            //Debug.Print(NI.IPAddress.ToString());
            _location = location;
            //NetworkInterface.EnableStaticIP("169.254.237.131", "255.255.0.0 ", "50-B7-C3-92-F9-5F");
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));

            _socket.Listen(Backlog);
            Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);
            _thread = new Thread(new ThreadStart(ListenForClients));
            _thread.Start();
        }
         public WebServer()

        {            
            //Initialize Socket class

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Request and bind to an IP from DHCP server

            _socket.Bind(new IPEndPoint(IPAddress.Any, 80));

            //Debug print our IP address

            Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);

            //Start listen for web requests

            _socket.Listen(10);
            
            ListenForRequest();

        }
         public void ListenForRequest()

        {

            while (true)

            {

                using (Socket clientSocket = _socket.Accept())

                {
                    
                    //Get clients IP

                    IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;

                    EndPoint clientEndPoint = clientSocket.RemoteEndPoint;

                    //int byteCount = cSocket.Available;

                    int bytesReceived = clientSocket.Available;

                    if (bytesReceived > 0)

                    {

                        //Get request

                        byte[] buffer = new byte[bytesReceived];

                        int byteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);

                        string request = new string(Encoding.UTF8.GetChars(buffer));

                        Debug.Print(request);

                        //Compose a response

                        string response = "Hello World";

                        string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";

                        clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                        clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                        //Blink the onboard LED
                        led.Write(true);
                        Thread.Sleep(150);
                        led.Write(false);

                    }

                }

            }

        }

        private void ListenForClients()
        {
            while (true)
            {
                using (Socket client = _socket.Accept())
                {
                    // Wait for data to become available
                    while (!client.Poll(10, SelectMode.SelectRead)) ;

                    int bytesSent = client.Available;
                    if (bytesSent > 0)
                    {
                        byte[] buffer = new byte[bytesSent];
                        int bytesReceived = client.Receive(buffer, bytesSent, SocketFlags.None);

                        if (bytesReceived == bytesSent)
                        {
                            string request = new string(Encoding.UTF8.GetChars(buffer));
                            Debug.Print(request);

                            Respond(client, request);
                        }
                    }
                }
            }
        }
        private void Respond(Socket client,string file)
        {
            //if (StringHelper.GetTextBetween(file, "GET", "HTTP").Equals("/index"))
            //{
            //    client.Send(HelloWorld, HelloWorld.Length, SocketFlags.None);
            //}
            SendFile(client, file);
        }
        private void SendFile(Socket client, string file)
        {
            if (File.Exists(file))
            {
                using (FileStream stream = File.Open(file, FileMode.Open))
                {
                    long fileSize = stream.Length;

                    string header = ResponseBegin + fileSize.ToString() + ResponseEnd;
                    client.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                    byte[] buffer = new byte[FileBufferLength];

                    while (stream.Position < stream.Length)
                    {
                        int xferSize = System.Math.Min(FileBufferLength, (int)(stream.Length - stream.Position));
                        stream.Read(buffer, 0, xferSize);
                        client.Send(buffer, xferSize, SocketFlags.None);
                    }
                }
            }
            else
            {
                SendErrorResponse(client);
            }
        }
        private void SendErrorResponse(Socket client)
        {
            string header = ResponseBegin + ErrorResponse.Length.ToString() + ResponseEnd;
            client.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);
            client.Send(ErrorResponse, ErrorResponse.Length, SocketFlags.None);
        }
        #region IDisposable
         ~WebServer()

        {

            Dispose();

        }

        public void Dispose()

        {

            if (_socket != null)

                _socket.Close();

        }

        #endregion


       
    }
}
