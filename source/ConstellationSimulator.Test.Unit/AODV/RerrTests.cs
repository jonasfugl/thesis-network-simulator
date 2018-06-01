using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using NUnit.Framework;
using ConstellationSimulator.Network.Message;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class RerrTests
    {
        private List<UnreachableDestination> destinations;
        private int NumberOfDestinations = 5;
        

        [SetUp]
        public void SetUp()
        {
            destinations = new List<UnreachableDestination>();
            for (int i = 0; i < NumberOfDestinations; i++)
            {
                var destination = "Destination" + i;
                destinations.Add(new UnreachableDestination(destination, i));
            }
        }

        [Test]
        public void Constructor_ValuesInitialized()
        {
            var uut = new Rerr(destinations);

            Assert.AreEqual(NumberOfDestinations, uut.DestinationCount);
            Assert.AreEqual(12 + NumberOfDestinations * 8, uut.Size);
            Assert.AreEqual(MessageType.Rerr, uut.Type);
        }

        [Test]
        public void CopyConstructor_ValuesInitialized()
        {
            var rerr = new Rerr(destinations);
            var uut = new Rerr(rerr);

            Assert.AreEqual(rerr.Size, uut.Size);
            Assert.AreEqual(rerr.Type, uut.Type);
            Assert.AreEqual(rerr.N, uut.N);
            Assert.AreEqual(rerr.DestinationCount, uut.DestinationCount);

            for (int i = 0; i < rerr.DestinationCount; i++)
            {
                Assert.AreEqual(rerr.UnreachableDestinations[i].UnreachableDestinationAddress, uut.UnreachableDestinations[i].UnreachableDestinationAddress);
                Assert.AreEqual(rerr.UnreachableDestinations[i].UnreachableDestinationSequenceNumber, uut.UnreachableDestinations[i].UnreachableDestinationSequenceNumber);
            }
        }

    }
}
