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
    class AodvParametersTets
    {

        [Test]
        public void Constructor_NetDiameterCorrect1020nonCross()
        {
            var uut = new AodvParameters(10, 10, 20, 10, false);

            Assert.AreEqual(19, uut.NetDiameter);
        }

        [Test]
        public void Constructor_NetDiameterCorrect1020Cross()
        {
            var uut = new AodvParameters(10, 10, 20, 10, true);

            Assert.AreEqual(15, uut.NetDiameter);
        }

        [Test]
        public void Constructor_NetDiameterCorrect520nonCross()
        {
            var uut = new AodvParameters(10, 10, 20, 5, false);

            Assert.AreEqual(14, uut.NetDiameter);
        }

        [Test]
        public void Constructor_NetDiameterCorrect520Cross()
        {
            var uut = new AodvParameters(10, 10, 20, 5, true);

            Assert.AreEqual(12, uut.NetDiameter);
        }
    }
}
