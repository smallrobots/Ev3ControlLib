//////////////////////////////////////////////////////////////////////////////////////////////////
// Hungry Anty Command Console                                                                  //
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
//                                                                                              //
// Visit http://wwww.smallrobots.it for tutorials and videos                                    //
//                                                                                              //
// Credits                                                                                      //
// The Hungry Anty is based on the Anty model of Laurent Valks described in                     //
// "The Lego Mindstorms Ev3 Discovery Book"                                                     //
//////////////////////////////////////////////////////////////////////////////////////////////////

using SmallRobots.Ev3ControlLib;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SmallRobots.Ev3ControlLib
{
    /// <summary>
    /// Class that connect to the Lego Mindstorms Ev3 Brick
    /// </summary>
    public class ConnectedRobotClient<Message> : INotifyPropertyChanged where Message : RobotMessage
    {
        #region Fields
        /// <summary>
        /// Signle log line
        /// </summary>
        string logLine;

        /// <summary>
        /// True if there is a valid connection to the Ev3
        /// </summary>
        bool isConnected;

        /// <summary>
        /// True when there is an attempt to connect to the Ev3 in progress
        /// </summary>
        bool isAttemptingConnection;

        /// <summary>
        /// Address of the Ev3
        /// </summary>
        IPAddress ipAddress;

        /// <summary>
        /// The port number for the remote device.
        /// </summary>
        const int port = 11000;

        /// <summary>
        /// ManualResetEvent instances signal completion - Connection estabilished
        /// </summary>
        static ManualResetEvent connectDone = new ManualResetEvent(false);

        /// <summary>
        /// ManualResetEvent instances signal completion - Send completede
        /// </summary>
        static ManualResetEvent sendDone = new ManualResetEvent(false);

        /// <summary>
        /// ManualResetEvent instances signal completion - Receive completede
        /// </summary>
        static ManualResetEvent receiveDone = new ManualResetEvent(false);

        /// <summary>
        /// The response from the remote device.
        /// </summary>
        static string response = string.Empty;

        Socket ev3Brick;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or protected sets the state of connection attempt in progress
        /// </summary>
        public bool IsAttemptingConnection
        {
            get
            {
                return isAttemptingConnection;
            }
            protected set
            {
                if (isAttemptingConnection != value)
                {
                    isAttemptingConnection = value;
                    RaisePropertyChanged("IsAttemptingConnection");
                }
            }
        }

        /// <summary>
        /// Gets or protected Sets the state of the connection to the Ev3
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
            protected set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    RaisePropertyChanged("IsConnected");
                }
            }
        }

        /// <summary>
        /// Gets os sets the IPAddress of the Lego Mindstorms Ev3 Brick
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
                    RaisePropertyChanged("IPAddress");
                }
            }
        }

        /// <summary>
        /// Gets or protected sets a single line of the connection log
        /// </summary>
        public string LogLine
        {
            get
            {
                return logLine;
            }
            protected set
            {
                if (logLine != value)
                {
                    logLine = value;
                    RaisePropertyChanged("LogLine");
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public ConnectedRobotClient() : base()
        {
            // Fields initialization
            init();
        }

        /// <summary>
        /// Initialize a ConnectRobotAddress specifying the IPAddress
        /// of the robot to connect to
        /// </summary>
        /// <param name="withRobotAddress">IP Address of the robo to connect to</param>
        public ConnectedRobotClient(IPAddress withRobotAddress) : base()
        {
            if (withRobotAddress == null)
            {
                throw new ArgumentNullException("withRobotAddress cannot be null");
            }
            else
            {
                // Fields initialization
                init();
                IPAddress = withRobotAddress;
            }
        }

        /// <summary>
        /// Fields initialization
        /// </summary>
        void init()
        {
            // Fields initialization
            logLine = string.Empty;
            isConnected = false;

            // Default IP Address
            ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
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

        #region Public methods
        /// <summary>
        /// Connects to the ConnectedRobot
        /// </summary>
        public void Connect()
        {
            //Thread th = new Thread(StartClient);
            //th.Start();
            StartClient();
        }

        /// <summary>
        /// Disconnect from the ConnectedRobot
        /// </summary>
        public void Disconnect()
        {
            IsConnected = false;
        }

        /// <summary>
        /// Sends a message to the ConnectedRobot
        /// </summary>
        /// <param name="thisMessage">Message to send</param>
        public void Send(RobotMessage thisMessage)
        {
            if (!IsConnected)
            {
                throw (new InvalidOperationException("The ConnectedRobotClient is not connected to the ConnectedRobot" +
                    "Please connect first"));
            }
            else
            {
                ev3Brick.Send(Encoding.UTF8.GetBytes(RobotMessage.Serialize(thisMessage)));
            }
        }

        public virtual RobotMessage Receive()
        {
            // Message received
            RobotMessage retValue = null;
            byte[] buffer = new byte[4096];

            // Checks connection
            if (!IsConnected)
            {
                throw (new InvalidOperationException("The ConnectedRobotClient is not connected to the ConnectedRobot" +
                    "Please connect first"));
            }
            else
            {
                ev3Brick.Receive(buffer);
            }

            string stringBuffer = Encoding.UTF8.GetString(buffer);

            retValue = RobotMessage.DeSerialize(data:stringBuffer, type: typeof(RobotMessage));

            // Return value
            return retValue;
        }

        ///// <summary>
        ///// Async
        ///// </summary>
        ///// <param name="message"></param>
        //public void Send(RobotMessage message)
        //{
        //    // Convert the string data to byte data using UTF8 encoding.
        //    byte[] byteData = Encoding.UTF8.GetBytes(RobotMessage.Serialize(message));

        //    // Begin sending the data to the remote device.
        //    ev3Brick.BeginSend(byteData, 0, byteData.Length, 0,
        //        new AsyncCallback(SendCallback), ev3Brick);
        //}


        #endregion

        #region Private methods
        //private static void SendCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        // Retrieve the socket from the state object.
        //        Socket client = (Socket)ar.AsyncState;

        //        // Complete sending the data to the remote device.
        //        int bytesSent = client.EndSend(ar);
        //        Console.WriteLine("Sent {0} bytes to server.", bytesSent);

        //        // Signal that all bytes have been sent.
        //        sendDone.Set();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        /// <summary>
        /// Connects to the Lego Mindstorms Ev3 Brick ConnectedRobot
        /// </summary>
        private void StartClient()
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP socket.
                ev3Brick = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                LogLine = "Attempting connection to Lego Mindstorms Ev3 Brick at address: " + ipAddress.ToString();
                IsAttemptingConnection = true;
                ev3Brick.Connect(remoteEP);
                //ev3Brick.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), ev3Brick);
                //connectDone.WaitOne();
                if (ev3Brick.Connected)
                {
                    IsConnected = true;
                }
                else
                {
                    IsConnected = false;
                }
                IsAttemptingConnection = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                LogLine = "Connection failed with message:";
                LogLine = e.ToString();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                if (client.Connected)
                {
                    // Complete the connection.
                    client.EndConnect(ar);

                    Console.WriteLine("Socket connected to {0}",
                        client.RemoteEndPoint.ToString());
                }
                else
                {
                    LogLine = "Unable to connect!";
                }

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }
}
