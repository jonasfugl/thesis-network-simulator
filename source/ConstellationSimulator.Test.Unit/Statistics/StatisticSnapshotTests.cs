using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.Statistics;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit
{
    [TestFixture]
    class StatisticSnapshotTests
    {
        [Test]
        public void TestProperties()
        {
            var pdr = 50.0;
            var minutes = "10";
            var gp = 18.3;
            uint genrreqs = 1337;

            var uut = new StatisticSnapshot
            {
                DataPacketDeliveryRate = pdr,
                GeneratedRreqs = genrreqs,
                Goodput = gp,
                MinutesIntoSim = minutes
            };

            Assert.AreEqual(pdr, uut.DataPacketDeliveryRate);
            Assert.AreEqual(minutes, uut.MinutesIntoSim);
            Assert.AreEqual(gp, uut.Goodput);
            Assert.AreEqual(genrreqs, uut.GeneratedRreqs);
        }
    }
}
