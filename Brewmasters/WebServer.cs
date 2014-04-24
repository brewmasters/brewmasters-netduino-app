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
using System.Collections;



namespace Brewmasters
{
    class WebServer : IDisposable
    {
        private JSONParser JSONSerializer = new JSONParser();
        private DateTime startTime = DateTime.Now;
        private const int Backlog = 10;
        private Socket _socket = null;
        public Socket lastSocket = null;
        private Thread _thread = null;
        //private string _location = null;
        private const int FileBufferLength = 1024;
        private const string ResponseBegin = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: ";
        private const string ResponseEnd = "\r\nConnection: close\r\n\r\n";
        private static byte[] ErrorResponse = Encoding.UTF8.GetBytes("<HTML><HEAD><TITLE>Website</TITLE></HEAD><BODY><H1>Content Not Found</H1></BODY></HTML>");
        private static byte[] HelloWorld = Encoding.UTF8.GetBytes("<HTML><HEAD><TITLE>Website</TITLE></HEAD><BODY><H1>Hello World</H1></BODY></HTML>");
        //private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        private Recipe currentRecipe = null;
       
       


        
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
            _thread = new Thread(new ThreadStart(ListenForRequest));
            _thread.Start();
            //ListenForRequest();
            

        }
         public void ListenForRequest()

        {

            while (true)

            {

                using (Socket clientSocket = _socket.Accept())

                {
                    lastSocket = clientSocket;
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
                        //if(request

                        Debug.Print(request);
                        //if (request.Equals("Connect"))
                        //{
                        //    clientSocket.Send(Encoding.UTF8.GetBytes(Program.isBrewing.ToString()), Program.isBrewing.ToString().Length, SocketFlags.None);
                        //}

                        //else
                        //{
                        //    if (!Program.isBrewing)
                        //    {
                        //        LoadRecipe(request);
                        //    }
                        //}
                        

                        //Compose a response

                        try{
                            String URL = StringHelper.GetTextBetween(request, "GET", "HTTP");
                            if (URL.Equals(""))
                            {
                                URL = StringHelper.GetTextBetween(request, "POST", "HTTP");
                            }
                            if (URL.Equals("status") || URL.Equals("/status"))
                            {
                                
                                string response = JSONSerializer.Serialize(Program.ResponseObj);
                                //string response = "Nice";
                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);
                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("recipe") || URL.Equals("/recipe"))
                            {
                            
                                LoadRecipe(request);
                                string response = "Recipe failed to load please try again";
                                if (Program.currentRecipe != null)
                                {
                                    Program.NextStep = true;
                                    Program.isBrewing = true;
                                    response = "Recipe Loaded Successfully";
                                }

                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
                            }
                            else if (URL.Equals("mashwaterconfirm") || URL.Equals("/mashwaterconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.PreMash) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Beginning Heating";
                                

                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
                            
                            }
                            else if (URL.Equals("openvalveconfirm") || URL.Equals("/openvalveconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.PreMash) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Beginning Pumping";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("closevalveconfirm") || URL.Equals("/closevalveconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.PreMash) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Beginning Mash";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("spargewaterconfirm") || URL.Equals("/spargewaterconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.Mashing) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Heating sparge water";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("spargevalveopenconfirm") || URL.Equals("/spargevalveopenconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.Sparging) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Beginning Sparge";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("spargevalvecloseconfirm") || URL.Equals("/spargevalvecloseconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.Sparging) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Beginning Boiling";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("hopsaddconfirm") || URL.Equals("/hopsaddconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.Boiling) && Program.isWaiting)
                                {
                                    //Program.isWaiting = false;
                                    Program.ingredientAdded = true;
                                }
                                string response = "Continuing Boiling";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("iceaddconfirm") || URL.Equals("/iceaddconfirm"))
                            {
                                if (Program.step.Equals(ProcessStep.Boiling) && Program.isWaiting)
                                {
                                    Program.isWaiting = false;
                                }
                                string response = "Finished with microcontroller process";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);

                            }
                            else if (URL.Equals("reset") || URL.Equals("/reset"))
                            {
                                Program.resetFlag = true;
                                string response = "Reset to Idle state";


                                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                                clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                                clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
                            }
                            
                        }
                        catch
                        {
                            string response = "An error has occured please try again";
                            string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                            clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                            clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
                        }
                        //Blink the onboard LED
                        //led.Write(true);
                        //Thread.Sleep(10);
                        //led.Write(false);

                    }

                }

            }

        }
        //Load the current Recipe
        private void LoadRecipe(String recipe)
        {
            
            Program.currentRecipe = JSONSerializer.Deserialize(recipe);
            Program.currentRecipe.mash_temperature += 12;
            
            
            //_thread.Abort();
        }
        //get the current Recipe from the webserver
        public Recipe getCurrentRecipe()
        {
            return this.currentRecipe;
        }
        public void SendResponse(string response)
        {

            if (lastSocket != null)
            {
                
                string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";

                lastSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);

                lastSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
            }
        }
        //Resets the current recipe called when process is completed or halted to allow the server to listen for a new request
        public void resetRecipe()
        {
            this.currentRecipe = null;
        }
        
       
       
        private void SendErrorResponse(Socket client)
        {
            string header = ResponseBegin + ErrorResponse.Length.ToString() + ResponseEnd;
            client.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);
            client.Send(ErrorResponse, ErrorResponse.Length, SocketFlags.None);
        }
        public void SendErrorResponse()
        {
            if (_socket != null)
            {
                string header = ResponseBegin + ErrorResponse.Length.ToString() + ResponseEnd;
                _socket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);
                _socket.Send(ErrorResponse, ErrorResponse.Length, SocketFlags.None);
            }
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
