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
using System.Net;
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
            localAddress = new IPAddress(new byte[4] { 172, 16, 232, 134 });
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

        class SpecialMessage : RobotMessage
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

    }
}
