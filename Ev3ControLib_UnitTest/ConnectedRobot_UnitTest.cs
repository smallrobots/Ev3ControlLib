/////////////////////////////////////////////////////////////////////////////////////////////////
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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallRobots.Ev3ControlLib;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ev3ControLib_UnitTest
{
    [TestClass]
    public class ConnectedRobot_UnitTest
    {
        // Default Ethernet Address for testing
        IPAddress localAddress;

        [TestInitialize]
        public void ConnectedRobot_UnitTest_Initialization()
        {
            localAddress = new IPAddress(new byte[4] { 192, 168, 1, 170 });
        }

        /// <summary>
        /// Default constructor - 1
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_1()
        {
            ConnectedRobot<RobotMessage> robot = new ConnectedRobot<RobotMessage>();
            Assert.IsNotNull(robot);
        }

        [Serializable]
        public class SpecialMessage : RobotMessage
        {
            public int testField;
        }

        /// <summary>
        /// Default constructor - 2
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_2()
        {
            ConnectedRobot<SpecialMessage> robot = new ConnectedRobot<SpecialMessage>();
            Assert.IsNotNull(robot);
        }

        /// <summary>
        /// Default constructor - 3
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_3()
        {
            ConnectedRobot<SpecialMessage> robot = new ConnectedRobot<SpecialMessage>(theAddress: localAddress);
            Assert.IsNotNull(robot);
            Assert.AreEqual(localAddress, robot.IPAddress);
        }

        /// <summary>
        /// Properties - 1 
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_4()
        {
            ConnectedRobot<SpecialMessage> robot = new ConnectedRobot<SpecialMessage>();
            Assert.IsNotNull(robot);

            robot.IPAddress = localAddress;
            Assert.AreEqual(localAddress, robot.IPAddress);
        }

        /// <summary>
        /// Starts and stops the server
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_5()
        {
            ConnectedRobot<SpecialMessage> robot = new ConnectedRobot<SpecialMessage>(theAddress: localAddress);
            Assert.IsNotNull(robot);
            Assert.AreEqual(localAddress, robot.IPAddress);
            Assert.AreEqual(false, robot.IsServerRunning);

            robot.Start();
            Thread.Sleep(100);
            Assert.AreEqual(true, robot.IsServerRunning);

            robot.Stop();
            Thread.Sleep(100);
            Assert.AreEqual(false, robot.IsServerRunning);
        }


        class SpecialRobot : ConnectedRobot<SpecialMessage>
        {
            public bool okIHaveReceivedAMessage = false;

            public SpecialRobot(IPAddress theAddress) : base(theAddress) { }

            protected override void ProcessLastReceivedMessage()
            {
                base.ProcessLastReceivedMessage();
                okIHaveReceivedAMessage = true;
            }
        }

        /// <summary>
        /// Starts the server and checks
        /// Send a message and checks
        /// Stops the server and checks
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_6()
        {
            SpecialRobot robot = new SpecialRobot(theAddress: localAddress);
            Assert.IsNotNull(robot);
            Assert.AreEqual(localAddress, robot.IPAddress);
            Assert.AreEqual(false, robot.IsServerRunning);

            Assert.IsTrue(!robot.okIHaveReceivedAMessage);

            // Starts the server and checks
            IPEndPoint remoteEP = new IPEndPoint(localAddress, 11000);
            robot.Start();
            Thread.Sleep(100);
            Assert.AreEqual(true, robot.IsServerRunning);

            // Connects to the server and check
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(remoteEP);
            Assert.AreEqual(client.Connected, true);

            // Create a message to send
            RobotMessage sentMessage = new RobotMessage();
            sentMessage.Sender = Sender.FromClient;
            string encodedMessage = RobotMessage.Serialize(theMessage: sentMessage);
            // Send the message
            int bytesSent = client.Send(Encoding.ASCII.GetBytes(encodedMessage));
            Assert.AreNotEqual(0, bytesSent);

            // Checks message received
            Thread.Sleep(100);
            Assert.IsTrue(robot.okIHaveReceivedAMessage);

            // Disconnects from the server and check
            client.Disconnect(false);
            Assert.AreEqual(client.Connected, false);

            // Stops the server and checks
            robot.Stop();
            Thread.Sleep(100);
            Assert.AreEqual(false, robot.IsServerRunning);
        }

        class SpecialRobot_2 : ConnectedRobot<SpecialMessage>
        {
            public bool okIHaveReceivedAMessage = false;

            public SpecialRobot_2(IPAddress theAddress) : base(theAddress) { }

            protected override void ProcessLastReceivedMessage()
            {
                base.ProcessLastReceivedMessage();
                okIHaveReceivedAMessage = true;

                // Sends a message back
                SpecialMessage message = new SpecialMessage();
                message.Sender = Sender.FromRobot;
                message.testField = 150;

                string encodedMessage = RobotMessage.Serialize(theMessage: message);

                // Send the message
                Ev3TCPServer.Send(encodedMessage);
            }
        }

        /// <summary>
        /// Starts the server and checks
        /// Send a message and checks
        /// The robot answers back with tesfield = 150;
        /// Stops the server and checks
        /// </summary>
        [TestMethod]
        public void ConnectedRobot_UnitTest_7()
        {
            SpecialRobot_2 robot = new SpecialRobot_2(theAddress: localAddress);
            Assert.IsNotNull(robot);
            Assert.AreEqual(localAddress, robot.IPAddress);
            Assert.AreEqual(false, robot.IsServerRunning);

            Assert.AreEqual(false, robot.okIHaveReceivedAMessage);

            // Starts the server and checks
            IPEndPoint remoteEP = new IPEndPoint(localAddress, 11000);
            robot.Start();
            Thread.Sleep(100);
            Assert.AreEqual(true, robot.IsServerRunning);

            // Connects to the server and check
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(remoteEP);
            Assert.AreEqual(client.Connected, true);

            // Create a message to send
            RobotMessage sentMessage = new RobotMessage();
            sentMessage.Sender = Sender.FromClient;
            string encodedMessage = RobotMessage.Serialize(theMessage: sentMessage);

            // Send the message
            int bytesSent = client.Send(Encoding.ASCII.GetBytes(encodedMessage));
            Assert.AreNotEqual(0, bytesSent);

            // Wait for answer
            byte[] buffer = new byte[4096];
            client.Receive(buffer);
            string receivedString = Encoding.ASCII.GetString(buffer);
            SpecialMessage receivedMessage = (SpecialMessage)RobotMessage.DeSerialize(receivedString, typeof(SpecialMessage));
            Assert.AreEqual(Sender.FromRobot, receivedMessage.Sender);
            Assert.AreEqual(150, receivedMessage.testField);

            // Checks message received
            Thread.Sleep(100);
            Assert.AreEqual(true, robot.okIHaveReceivedAMessage);

            // Disconnects from the server and check
            client.Disconnect(false);
            Assert.AreEqual(client.Connected, false);

            // Stops the server and checks
            robot.Stop();
            Thread.Sleep(100);
            Assert.AreEqual(false, robot.IsServerRunning);
        }
    }
}
