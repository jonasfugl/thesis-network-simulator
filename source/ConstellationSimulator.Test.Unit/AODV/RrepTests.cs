using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Network.Message;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class RrepTests
    {
        [Test]
        public void Constructor_ValuesInitialized()
        {
            var uut = new Rrep();

            Assert.AreEqual(MessageType.Rrep, uut.Type);
            Assert.IsFalse(uut.R);
            Assert.IsFalse(uut.A);
            Assert.AreEqual(0, uut.PrefixSize);
            Assert.AreEqual(0, uut.HopCount);
            Assert.AreEqual(20, uut.Size);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            var rrep = new Rrep
            {
                R = false,
                A = false,
                PrefixSize = 10,
                HopCount = 11,
                DestinationAddress = "Destination",
                DestinationSequenceNumber = 12,
                OriginatorAddress = "Originator",
                Lifetime = DateTime.MaxValue
            };

            var uut = new Rrep(rrep);
            Assert.AreEqual(rrep.R, uut.R);
            Assert.AreEqual(rrep.A, uut.A);
            Assert.AreEqual(rrep.PrefixSize, uut.PrefixSize);
            Assert.AreEqual(rrep.HopCount, uut.HopCount);
            Assert.AreEqual(rrep.DestinationAddress, uut.DestinationAddress);
            Assert.AreEqual(rrep.DestinationSequenceNumber, uut.DestinationSequenceNumber);
            Assert.AreEqual(rrep.OriginatorAddress, uut.OriginatorAddress);
            Assert.AreEqual(rrep.Lifetime, uut.Lifetime);
        }
    }
}
