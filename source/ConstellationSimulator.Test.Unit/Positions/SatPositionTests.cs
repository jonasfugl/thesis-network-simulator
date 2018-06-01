using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.Positions;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit
{
    [TestFixture]
    class SatPositionTests
    {
        private int orbit = 2;
        private int sat = 4;

        [Test]
        public void Constructor_OrbitSatCorrectly()
        {
            var res = new SatPosition(orbit, sat);
            Assert.AreEqual(orbit, res.OrbitNumber);
            Assert.AreEqual(sat, res.SatNumber);
        }
    }
}
