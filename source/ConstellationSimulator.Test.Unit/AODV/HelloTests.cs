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
    class HelloTests
    {
        private string destination = "destination";
        private int dsn = 10;
        private int lifetime = 15;


        [Test]
        public void Constructor_ValuesInitialized()
        {
            var uut = new Hello(destination, dsn, lifetime);

            Assert.AreEqual(destination, uut.DestinationAddress);
            Assert.AreEqual(dsn, uut.DestinationSequenceNumber);
            Assert.AreEqual(0, uut.HopCount);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            var hello = new Hello(destination, dsn, lifetime);
            var uut = new Hello(hello);

            Assert.AreEqual(hello.Type, uut.Type);
            Assert.AreEqual(hello.Size, uut.Size);
            Assert.AreEqual(hello.DestinationAddress, uut.DestinationAddress);
            Assert.AreEqual(hello.DestinationSequenceNumber, uut.DestinationSequenceNumber);
            Assert.AreEqual(hello.HopCount, uut.HopCount);
            Assert.AreEqual(hello.Lifetime, uut.Lifetime);
        }
    }
}
