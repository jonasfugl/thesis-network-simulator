using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class UnreachableDestinationTests
    {
        private string destination = "destination";
        private int sn = 10;

        [Test]
        public void Constructor_ValuesInitialized()
        {
            var uut = new UnreachableDestination(destination, sn);
            Assert.AreEqual(destination, uut.UnreachableDestinationAddress);
            Assert.AreEqual(sn, uut.UnreachableDestinationSequenceNumber);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            var ud = new UnreachableDestination(destination, sn);
            var uut = new UnreachableDestination(ud);

            Assert.AreEqual(ud.UnreachableDestinationAddress, uut.UnreachableDestinationAddress);
            Assert.AreEqual(ud.UnreachableDestinationSequenceNumber, uut.UnreachableDestinationSequenceNumber);
        }
    }
}
