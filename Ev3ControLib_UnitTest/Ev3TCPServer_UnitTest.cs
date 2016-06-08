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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmallRobots.Ev3ControlLib;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ev3ControLib_UnitTest
{
    [TestClass]
    public partial class Ev3TCPServer_UnitTest
    {
        // Default Ethernet Address for testing
        IPAddress localAddress;

        [TestInitialize]
        public void Ev3TCPServer_UnitTest_Initialization()
        {
            localAddress = new IPAddress(new byte[4] { 192, 168, 1, 170 });
        }

        /// <summary>
        /// Default constructor test
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_1()
        {
            Ev3TCPServer server = new Ev3TCPServer();
            Assert.IsNotNull(server);
            Assert.AreEqual(server.IsRunning, false);
        }

        /// <summary>
        /// Test of the constructors accepting the IP Address
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_1_2()
        {
            IPAddress ipAddress = new IPAddress(new byte[4]{ 192,168,35,10});

            Ev3TCPServer server = new Ev3TCPServer(withIPAddress: ipAddress);
            Assert.IsNotNull(server);
            Assert.AreEqual(ipAddress, server.IPAddress);           

        }

        /// <summary>
        /// Simple robot class used for tests
        /// </summary>
        class RobotWithTCPServer : Robot
        {
            public Ev3TCPServer Server { get; set; }

            public RobotWithTCPServer()
            {
                Server = new Ev3TCPServer();
            }
        }

        /// <summary>
        /// Create a robot with the Ev3TCPServer
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_2()
        {
            RobotWithTCPServer robotWithTCPServer = new RobotWithTCPServer();
            Assert.IsNotNull(robotWithTCPServer);
            Assert.IsNotNull(robotWithTCPServer.Server);
        }

        /// <summary>
        /// IP Address property test
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_3()
        {
            Ev3TCPServer server = new Ev3TCPServer();

            // Starts the server and checks
            IPAddress ipTest = new IPAddress(new byte[4] { 192, 168, 0, 10 });
            server.IPAddress = ipTest;

            Assert.AreEqual(ipTest, server.IPAddress);
        }

        /// <summary>
        /// Starts and stops the Server
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_4()
        {
            // Server Ip Address and server initialization
            //IPAddress ipTest = new IPAddress(new byte[4] { 192, 168, 1, 170 });
            Ev3TCPServer server = new Ev3TCPServer(withIPAddress: localAddress);
            
            // Starts the server and checks
            server.Start();

            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, true);

            // Stops the server and checks
            server.Stop();

            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, false);
        }

        /// <summary>
        /// Starts and stops the Server and connects to it
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_5()
        {
            // Server Ip Address and server initialization
            // IPAddress ipTest = new IPAddress(new byte[4] { 192, 168, 1, 170 });
            Ev3TCPServer server = new Ev3TCPServer(withIPAddress: localAddress);
            IPEndPoint remoteEP = new IPEndPoint(localAddress, 11000);

            // Starts the server and checks
            server.Start();
            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, true);

            // Connects to the server and check
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(remoteEP);
            Assert.AreEqual(client.Connected, true);

            // Disconnects from the server and check
            client.Disconnect(false);
            Assert.AreEqual(client.Connected, false);

            // Stops the server and checks
            server.Stop();
            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, false);
        }

        /// <summary>
        /// Robot message constructor
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_6()
        {
            RobotMessage message = new RobotMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual(Sender.Undefined, message.Sender);
        }

        /// <summary>
        /// Robot message Properties
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_7()
        {
            RobotMessage message = new RobotMessage();
            Assert.IsNotNull(message);
            Assert.AreEqual(Sender.Undefined, message.Sender);

            message.Sender = Sender.FromClient;
            Assert.AreEqual(Sender.FromClient, message.Sender);

            message.Sender = Sender.FromRobot;
            Assert.AreEqual(Sender.FromRobot, message.Sender);

            message.Sender = Sender.Undefined;
            Assert.AreEqual(Sender.Undefined, message.Sender);
        }

        /// <summary>
        /// Robot message Serialize / Deserialize "FromCLient"
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_8()
        {
            RobotMessage sentMessage = new RobotMessage();
            sentMessage.Sender = Sender.FromClient;

            string messageSerialized = RobotMessage.Serialize(theMessage: sentMessage);
            Assert.IsNotNull(messageSerialized);
            Assert.AreNotEqual(string.Empty, messageSerialized);

            RobotMessage receivedMessage = RobotMessage.DeSerialize(data: messageSerialized, type: typeof(RobotMessage));
            Assert.AreEqual(sentMessage.Sender, receivedMessage.Sender);
        }

        /// <summary>
        /// Robot message Serialize / Deserialize "FromRobot"
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_9()
        {
            RobotMessage sentMessage = new RobotMessage();
            sentMessage.Sender = Sender.FromRobot;

            string messageSerialized = RobotMessage.Serialize(theMessage: sentMessage);
            Assert.IsNotNull(messageSerialized);
            Assert.AreNotEqual(string.Empty, messageSerialized);

            RobotMessage receivedMessage = RobotMessage.DeSerialize(data: messageSerialized, type: typeof(RobotMessage));
            Assert.AreEqual(sentMessage.Sender, receivedMessage.Sender);
        }

        /// <summary>
        /// Robot message Serialize / Deserialize "Undefined"
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_10()
        {
            RobotMessage sentMessage = new RobotMessage();
            sentMessage.Sender = Sender.Undefined;

            string messageSerialized = RobotMessage.Serialize(theMessage: sentMessage);
            Assert.IsNotNull(messageSerialized);
            Assert.AreNotEqual(string.Empty, messageSerialized);

            RobotMessage receivedMessage = RobotMessage.DeSerialize(data: messageSerialized, type: typeof(RobotMessage));
            Assert.AreEqual(sentMessage.Sender, receivedMessage.Sender);
        }
    
        [Serializable]
        public class OtherRobotMessage : RobotMessage
        {
            #region Fields
            public int testField;
            #endregion
        }

        /// <summary>
        /// Robot message Serialize / Deserialize Derived Class
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_11()
        {
            OtherRobotMessage sentMessage = new OtherRobotMessage();
            sentMessage.Sender = Sender.FromRobot;
            sentMessage.testField = 58;

            string messageSerialized = RobotMessage.Serialize(theMessage: sentMessage);
            Assert.IsNotNull(messageSerialized);
            Assert.AreNotEqual(string.Empty, messageSerialized);

            OtherRobotMessage receivedMessage = (OtherRobotMessage) RobotMessage.DeSerialize(data: messageSerialized, type: typeof(OtherRobotMessage));
            Assert.AreEqual(sentMessage.Sender, receivedMessage.Sender);
            Assert.AreEqual(sentMessage.testField, receivedMessage.testField);
        }

        /// <summary>
        /// Starts and stops the Server and connects to it
        /// Then sends a message and checks
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_12()
        {
            // Server Ip Address and server initialization
            // IPAddress ipTest = new IPAddress(new byte[4] { 192, 168, 1, 170 });
            Ev3TCPServer server = new Ev3TCPServer(withIPAddress: localAddress);
            IPEndPoint remoteEP = new IPEndPoint(localAddress, 11000);

            // Starts the server and checks
            server.Start();
            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, true);

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
            Assert.IsNotNull(server.LastMessage);
            Assert.AreNotEqual(string.Empty, server.LastMessage);
            Assert.AreEqual(sentMessage.Sender, RobotMessage.DeSerialize(server.LastMessage, typeof(RobotMessage)).Sender);

            // Disconnects from the server and check
            client.Disconnect(false);
            Assert.AreEqual(client.Connected, false);

            // Stops the server and checks
            server.Stop();
            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, false);
        }

        /// <summary>
        /// Starts and stops the Server and connects to it
        /// Then sends a message and checks
        /// Checks also the Last Message Changed event
        /// </summary>
        [TestMethod]
        public void Ev3TCPServer_UnitTest_13()
        {
            // Server Ip Address and server initialization
            Ev3TCPServer server = new Ev3TCPServer(withIPAddress: localAddress);
            IPEndPoint remoteEP = new IPEndPoint(localAddress, 11000);

            bool propertyChangedEventFired = false;
            server.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                                     {
                                         propertyChangedEventFired = true;
                                     };

            // Starts the server and checks
            server.Start();
            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, true);

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
            Assert.IsNotNull(server.LastMessage);
            Assert.AreNotEqual(string.Empty, server.LastMessage);
            Assert.AreEqual(sentMessage.Sender, RobotMessage.DeSerialize(server.LastMessage, typeof(RobotMessage)).Sender);

            // Checks event fired
            Assert.AreEqual(true, propertyChangedEventFired);

            // Disconnects from the server and check
            client.Disconnect(false);
            Assert.AreEqual(client.Connected, false);

            // Stops the server and checks
            server.Stop();
            Thread.Sleep(100);
            Assert.AreEqual(server.IsRunning, false);
        }
    }
}
