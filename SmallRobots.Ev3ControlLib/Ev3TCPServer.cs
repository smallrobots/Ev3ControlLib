﻿//////////////////////////////////////////////////////////////////////////////////////////////////
// Ev3 Control Library (Ev3ControlLib)                                                          //
// Version 1.0                                                                                  //
//                                                                                              //
// Happily shared under the MIT License (MIT)                                                   //
//                                                                                              //
// Copyright(c) 2016 SmallRobots.it                                                             //
//                                                                                              //
// Permission is hereby granted, free of charge, to any person obtaining                        //
//a copy of this software and associated documentation files (the "Software"),                  //
// to deal in the Software without restriction, including without limitation the rights         //
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies             //
// of the Software, and to permit persons to whom the Software is furnished to do so,           //      
// subject to the following conditions:                                                         //
//                                                                                              //
// The above copyright notice and this permission notice shall be included in all               //
// copies or substantial portions of the Software.                                              //
//                                                                                              //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,          //
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR     //
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE           //
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,          //
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE        //
// OR OTHER DEALINGS IN THE SOFTWARE.                                                           //
//////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace SmallRobots.Ev3ControlLib
{
    /// <summary>
    /// Enumeration of message senders type
    /// </summary>
    public enum Sender
    {
        FromClient = 0,
        FromRobot,
        Undefined
    }

    /// <summary>
    /// Message sent between the robot and its client
    /// </summary>
    [Serializable]
    public class RobotMessage
    {
        #region Fields
        /// <summary>
        /// Sender type
        /// </summary>
        Sender sender;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the sender type
        /// </summary>
        public Sender Sender
        {
            get
            {
                return sender;
            }
            set
            {
                if (sender != value)
                {
                    sender = value;
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public RobotMessage()
        {
            // Fields initialization
            sender = Sender.Undefined;
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Serializes the message to xml formats to be recreated at the destination
        /// </summary>
        /// <param name="theMessage">Message to be encoded</param>
        /// <returns>String representing the encoded message</returns>
        public static string Serialize(RobotMessage theMessage)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(theMessage.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, theMessage);
                return textWriter.ToString();
            }
        }

        /// <summary>
        /// Deserializes the message from the xml format
        /// </summary>
        /// <param name="data">String contained the encoded message</param>
        /// <param name="type">Type of the encoded message (if a derived class)</param>
        /// <returns></returns>
        public static RobotMessage DeSerialize(string data, Type type)
        {
            var serializer = new XmlSerializer(type);
            RobotMessage result;

            using (TextReader reader = new StringReader(data))
            {
                result = (RobotMessage)serializer.Deserialize(reader);
            }

            return result;
        }
        #endregion

    }

    /// <summa
    /// Message state
    /// </summary>
    public class TCPMessageState
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    /// <summary>
    /// Class that implements a TCP Asynchronous Socket Server
    /// </summary>
    public partial class Ev3TCPServer : INotifyPropertyChanged
    {
        #region Fields
        ManualResetEvent allDone = new ManualResetEvent(false);

        /// <summary>
        /// Server Thread
        /// </summary>
        Thread serverThread;

        /// <summary>
        /// Last client to call this server
        /// </summary>
        Socket lastCallerClient;

        /// <summary>
        /// True when the server stop request has been issued
        /// </summary>
        bool serverStopRequest;

        /// <summary>
        /// True when the server is running
        /// </summary>
        bool isRunning;

        /// <summary>
        /// Server IP Address
        /// </summary>
        IPAddress ipAddress;

        /// <summary>
        /// Last received message
        /// </summary>
        string lastMessage;

        /// <summary>
        /// The server socket
        /// </summary>
        Socket serverSocket;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or protected sets the last message received from this
        /// server
        /// </summary>
        public string LastMessage
        {
            get
            {
                return lastMessage;
            }
            protected set
            {
                if (lastMessage != value)
                {
                    lastMessage = value;
                    RaisePropertyChanged("LastMessage");
                }
            }
        }

        /// <summary>
        /// Gets or sets the server ip address
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                return ipAddress;
            }
            set
            {
                if (ipAddress != value)
                {
                    ipAddress = value;
                }
            }
        }

        /// <summary>
        /// Gets and protected sets the running state of the server
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
            protected set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                }
            }
        } 
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public Ev3TCPServer()
        {
            // Fiedls initialization
            init();
        }

        public Ev3TCPServer(IPAddress withIPAddress)
        {
            // Fields initialization
            init();

            IPAddress = withIPAddress;
        }

        /// <summary>
        /// Fiedls initialization
        /// </summary>
        private void init()
        {
            // Fiedls initialization
            isRunning = false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Server Thread
        /// </summary>
        public void ServerThread()
        {
            // Voids the stop request
            serverStopRequest = false;

            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket.
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress, 11000);

            // Create a TCP/IP socket.
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                serverSocket.Bind(localEndPoint);
                serverSocket.Listen(100);

                // Server started
                IsRunning = true;

                while (!serverStopRequest)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    serverSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        serverSocket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

                // Closes the socket
                if (serverSocket.Connected)
                {
                    serverSocket.Shutdown(SocketShutdown.Both);
                }
                serverSocket.Close();
                IsRunning = false;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            TCPMessageState state = new TCPMessageState();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, TCPMessageState.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            TCPMessageState state = (TCPMessageState)ar.AsyncState;
            lastCallerClient = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = lastCallerClient.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                LastMessage = content;
                //if (content.IndexOf("<EOF>") > -1)
                //{
                //    // All the data has been read from the 
                //    // client. Display it on the console.
                //    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                //        content.Length, content);
                //    // Echo the data back to the client.
                //    // Send(handler, content);
                //    LastMessage = content;
                //}
                //else
                //{
                //    // Not all data received. Get more.
                //    handler.BeginReceive(state.buffer, 0, TCPMessageState.BufferSize, 0,
                //    new AsyncCallback(ReadCallback), state);
                //}
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the server
        /// </summary>
        public void Start()
        {
            serverThread = new Thread(ServerThread);
            serverThread.Start();
        }

        /// <summary>
        /// Stops the server
        /// </summary>
        public void Stop()
        {
            // Stops the server
            serverStopRequest = true;
            allDone.Set();

            // Joins the server thread
            serverThread.Join();
        }

        /// <summary>
        /// Sends the supplied data to the last client that 
        /// called this server
        /// </summary>
        /// <param name="data">Data to send</param>
        public void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            lastCallerClient.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), serverSocket);
        }
        #endregion

        #region INotifyPropertyChanged interface implementation
        /// <summary>
        /// Event raised when a property changes its value
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event if a handler is assigned
        /// </summary>
        /// <param name="propertyName">Property for which the event must be raised</param>
        protected void RaisePropertyChanged(string propertyName = "")
        {
            // Raises the event if the handler is not null
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return;
        }
        #endregion
    }
}
