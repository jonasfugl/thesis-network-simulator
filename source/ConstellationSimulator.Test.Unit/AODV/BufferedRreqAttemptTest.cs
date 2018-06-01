using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class BufferedRreqAttemptTest
    {
        [Test]
        public void Constructor_ValuesInitialized()
        {
            var attemptNo = 1337;
            var expire = 1000;

            var destination = "Destination";
            var originator = "Originator";
            var rreqId = 1338;

            var rreq = new Rreq();
            rreq.DestinationAddress = destination;
            rreq.OriginatorAddress = originator;
            rreq.RouteRequestId = rreqId;
            

            var uut = new BufferedRreqAttempt(attemptNo, rreq, expire);

            Assert.AreEqual(destination, uut.DestinationAddress);
            Assert.AreEqual(originator, uut.OriginatorAddress);
            Assert.AreEqual(attemptNo, uut.AttemptNo);
            Assert.AreEqual(0, uut.RetryNo);
            Assert.AreEqual(rreqId, uut.RouteRequestId);
        }
    }
}
