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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Ev3ControLib_UnitTest
{
    [TestClass]
    public class ConnectedRobotClient_UnitTest
    {
        IPAddress localAddress;

        [TestInitialize]
        public void ConnectedRobotClient_UnitTest_Initialization()
        {
            // localAddress = new IPAddress(new byte[4] { 192, 168, 1, 170 });
            localAddress = new IPAddress(new byte[4] { 172, 16, 232, 134 });
        }

        /// <summary>
        /// Default constructor test
        /// </summary>
        [TestMethod]
        public void ConnectedRobotClient_UnitTest_1()
        {
            ConnectedRobotClient<RobotMessage> crl = new ConnectedRobotClient<RobotMessage>();
            Assert.IsNotNull(crl);
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        [TestMethod]
        public void ConnectedRobotClient_UnitTest_2()
        {
            IPAddress theRobotAddress = localAddress;
            ConnectedRobotClient<RobotMessage> crl = new ConnectedRobotClient<RobotMessage>(withRobotAddress: theRobotAddress);
            Assert.IsNotNull(crl);
        }

        /// <summary>
        /// Constructor with parameter (exception)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectedRobotClient_UnitTest_3()
        {
            IPAddress theRobotAddress = null;
            ConnectedRobotClient<RobotMessage> crl = new ConnectedRobotClient<RobotMessage>(withRobotAddress: theRobotAddress);
        }

        /// <summary>
        /// Connects to a Connected Robot
        /// </summary>
        [TestMethod]
        public void ConnectedRobotClient_UnitTest_4()
        {
            ConnectedRobot<RobotMessage> robot = new ConnectedRobot<RobotMessage>(theAddress: localAddress);
            ConnectedRobotClient<RobotMessage> client = new ConnectedRobotClient<RobotMessage>(withRobotAddress: localAddress);

            Assert.IsNotNull(robot);
            Assert.IsNotNull(client);
            Assert.IsTrue(!client.IsConnected);

            robot.Start();
            client.Connect();

            Assert.IsTrue(client.IsConnected);
        }

        /// <summary>
        /// Client sends a RobotMessage and the robot 
        /// answers a RobotMessage back
        /// </summary>
        [TestMethod]
        public void ConnectedRobotClient_UnitTest_5()
        {
            ConnectedRobot<RobotMessage> robot = new ConnectedRobot<RobotMessage>(theAddress: localAddress);
            ConnectedRobotClient<RobotMessage> client = new ConnectedRobotClient<RobotMessage>(withRobotAddress: localAddress);

            Assert.IsNotNull(robot);
            Assert.IsNotNull(client);
            Assert.IsTrue(!client.IsConnected);

            robot.Start();
            client.Connect();
            Assert.IsTrue(client.IsConnected);

            RobotMessage message = new RobotMessage();
            message.Sender = Sender.FromClient;

            client.Send(message);
            RobotMessage answer = client.Receive();

            Assert.AreEqual(Sender.FromRobot, answer.Sender);
        }
    }
}
