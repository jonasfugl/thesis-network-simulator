using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using ConstellationSimulator.Helpers;
using ConstellationSimulator.Network;
using ConstellationSimulator.Network.Message;
using ConstellationSimulator.Statistics;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.Network
{
    [TestFixture]
    class LinkLayerTests
    {
        private LinkLayer _uut;

        [SetUp]
        public void SetUp()
        {
            var conf = new SimulationConfiguration();
            conf.Initialize();
            conf.EnableSatelliteLogging = false;

            var results = new StatisticsCollector();
            results.NewSimulation();

            var logger = new Logger(string.Empty, conf);
            
            _uut = new LinkLayer(conf, null, null, logger, results);
        }

        [Test]
        public void IncomingMessageCount_MessageSent()
        {
            Assert.AreEqual(0, _uut.IncomingMessageCount);

            _uut.ReceiveMessage(new Message(new Rrepack(), "destination", "source", 10));

            Assert.AreEqual(1, _uut.IncomingMessageCount);
        }

        [Test]
        public void SendMessage_TtlExpired()
        {
            var expiredMsg = new Message(new Rrepack(), "destination", "source", 0);

            var result = _uut.SendMessage(expiredMsg, "nextHop");

            Assert.AreEqual(LinkLayerResultType.TtlExpired, result.Result);
        }

        [Test]
        public void GetIncomingMessage_NoMessages()
        {
            var msg = _uut.GetIncomingMessage();

            Assert.IsNull(msg);
        }

        [Test]
        public void GetIncomingMessage_MessageReturned()
        {
            Assert.AreEqual(0, _uut.IncomingMessageCount);

            var newMsg = new Message(new Rrepack(), "destination", "source", 10);
            _uut.ReceiveMessage(newMsg);


            var msg = _uut.GetIncomingMessage();
            Assert.AreEqual(newMsg.SourceAddress, msg.SourceAddress);
            Assert.AreEqual(newMsg.DestinationAddress, msg.DestinationAddress);
        }
    }
}
