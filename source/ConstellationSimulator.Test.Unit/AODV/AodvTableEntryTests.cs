using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.AODV;
using ConstellationSimulator.Configuration;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit.AODV
{
    [TestFixture]
    class AodvTableEntryTests
    {
        private AodvTableEntry _uut;
        private SimulationConfiguration conf = new SimulationConfiguration();

        private string destination = "destination";
        private int dsn = 10;
        private bool validDsn = true;
        private int hops = 11;
        private bool valid = true;
        private string nexthop = "nextHop";

        [SetUp]
        public void SetUp()
        {
            _uut = new AodvTableEntry()
            {
                DestinationAddress = destination,
                DestinationSequenceNumber = dsn,
                ExpirationTime = DateTime.MaxValue,
                HopCount = hops,
                NextHop = nexthop,
                Precursors = new List<string>(),
                Valid = valid,
                ValidDestinationSequenceNumberFlag = validDsn
            };

            conf.Initialize();
        }

        [Test]
        public void Properties()
        {
            Assert.AreEqual(destination, _uut.DestinationAddress);
            Assert.AreEqual(dsn, _uut.DestinationSequenceNumber);
            Assert.AreEqual(DateTime.MaxValue, _uut.ExpirationTime);
            Assert.AreEqual(hops, _uut.HopCount);
            Assert.AreEqual(nexthop, _uut.NextHop);
            Assert.AreEqual(valid,_uut.Valid);
            Assert.AreEqual(validDsn, _uut.ValidDestinationSequenceNumberFlag);
        }

        [Test]
        public void Invalidate_EntryIsInvalidated()
        {
            _uut.Invalidate(conf);

            Assert.IsFalse(_uut.Valid);
        }

        [Test]
        public void SetActive_EntryIsActivated()
        {
            _uut.SetActive(conf);

            Assert.IsTrue(_uut.Valid);
        }
    }
}
