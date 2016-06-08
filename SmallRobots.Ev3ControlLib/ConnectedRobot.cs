//////////////////////////////////////////////////////////////////////////////////////////////////
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
using System.Net;

namespace SmallRobots.Ev3ControlLib
{
    /// <summary>
    /// Robot with an embedded TCPServer
    /// </summary>
    /// <typeparam name="Message"></typeparam>
    public class ConnectedRobot<Message> : Robot where Message : RobotMessage
    {
        #region Fields
        /// <summary>
        /// Embedded Ev3TCPServer
        /// </summary>
        Ev3TCPServer ev3TCPServer;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the embedded Ev3TCPServer
        /// </summary>
        protected Ev3TCPServer Ev3TCPServer
        {
            get
            {
                return ev3TCPServer;
            }
            set
            {
                if (ev3TCPServer != value)
                {
                    ev3TCPServer = value;
                }
            }
        }

        /// <summary>
        /// Gets the state of the embedded Ev3TCPServer
        /// </summary>
        public bool IsServerRunning
        {
            get
            {
                return Ev3TCPServer.IsRunning;
            }
        }

        /// <summary>
        /// Gets or sets the Ip Address of the Connected Robot
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                return Ev3TCPServer.IPAddress;
            }
            set
            {
                if (Ev3TCPServer.IPAddress != value)
                {
                    Ev3TCPServer.IPAddress = value;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ConnectedRobot()
        {
            // Fields initialization
            init();
        }

        /// <summary>
        /// Construct a ConnectedRobot with the embedded Ev3TCPServer
        /// at the specified address
        /// </summary>
        /// <param name="theAddress">IP Address of the ConnectedRobot</param>
        public ConnectedRobot(IPAddress theAddress)
        {
            // Fieds initialization
            init(withAddress: theAddress);
        }

        /// <summary>
        /// Fields on
        /// </summary>
        private void init(IPAddress withAddress = null)
        {
            if (withAddress != null)
            {
                ev3TCPServer = new Ev3TCPServer(withAddress);
            }
            else
            {
                ev3TCPServer = new Ev3TCPServer();
            }

            // Subscribe the PropertyChanged Evenet
            Ev3TCPServer.PropertyChanged += Ev3TCPServer_PropertyChanged;

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Starts the robot by starting the embedded Ev3TCPServer
        /// </summary>
        public virtual void Start()
        {
            // Starts the server
            Ev3TCPServer.Start();
        }

        /// <summary>
        /// Stops the robot by stopping the embedded Ev3TCPServer
        /// </summary>
        public virtual void Stop()
        {
            // Starts the server
            Ev3TCPServer.Stop();
        }
        #endregion

        #region Protected methods
        protected void Ev3TCPServer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName=="LastMessage")
            {
                // Call the relative handler
                ProcessLastReceivedMessage();
            }
        }

        /// <summary>
        /// Processes the last received message
        /// </summary>
        protected virtual void ProcessLastReceivedMessage()
        {

        }
        #endregion
    }
}
