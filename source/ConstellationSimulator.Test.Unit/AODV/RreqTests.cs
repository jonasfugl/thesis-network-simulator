using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Network.Message;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class RreqTests
    {

        [Test]
        public void Constructor_ValuesInitialized()
        {
            var uut = new Rreq();

            Assert.AreEqual(MessageType.Rreq, uut.Type);
            Assert.AreEqual(24, uut.Size);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            var rreq = new Rreq();
            var uut = new Rreq(rreq);

            Assert.AreEqual(rreq.Size, uut.Size);
            Assert.AreEqual(rreq.Type, uut.Type);
            Assert.AreEqual(rreq.J, uut.J);
            Assert.AreEqual(rreq.R, uut.R);
            Assert.AreEqual(rreq.G, uut.G);
            Assert.AreEqual(rreq.D, uut.D);
            Assert.AreEqual(rreq.U, uut.U);
            Assert.AreEqual(rreq.HopCount, uut.HopCount);
            Assert.AreEqual(rreq.RouteRequestId, uut.RouteRequestId);
            Assert.AreEqual(rreq.DestinationAddress, uut.DestinationAddress);
            Assert.AreEqual(rreq.DestinationSequenceNumber, uut.DestinationSequenceNumber);
            Assert.AreEqual(rreq.OriginatorAddress, uut.OriginatorAddress);
            Assert.AreEqual(rreq.OriginatorSequenceNumber, uut.OriginatorSequenceNumber);
        }
    }
}
