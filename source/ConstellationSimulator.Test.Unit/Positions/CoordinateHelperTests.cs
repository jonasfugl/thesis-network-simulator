using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConstellationSimulator.Positions;
using NUnit.Framework;

namespace ConstellationSimulator.Test.Unit
{
    class CoordinateHelperTests
    {
        private Coordinate neCoord = new Coordinate(60, 90, 400);
        private Coordinate seCoord = new Coordinate(-60, 120, 400);
        private Coordinate nwCoord = new Coordinate(90, -90, 400);
        private Coordinate swCoord = new Coordinate(-60, -60, 400);
        private Coordinate unknownCoord = new Coordinate(-160, 660, 400);

        [Test]
        public void CalcHemisphere_NorthEast()
        {
            var res = CoordinateHelper.CalcHemisphere(neCoord);
            Assert.AreEqual(CoordinateHelper.Hemisphere.NorthEast, res);
        }

        [Test]
        public void CalcHemisphere_NorthWest()
        {
            var res = CoordinateHelper.CalcHemisphere(nwCoord);
            Assert.AreEqual(CoordinateHelper.Hemisphere.NorthWest, res);
        }

        [Test]
        public void CalcHemisphere_SouthEast()
        {
            var res = CoordinateHelper.CalcHemisphere(seCoord);
            Assert.AreEqual( CoordinateHelper.Hemisphere.SouthEast, res);
        }

        [Test]
        public void CalcHemisphere_SouthWest()
        {
            var res = CoordinateHelper.CalcHemisphere(swCoord);
            Assert.AreEqual(CoordinateHelper.Hemisphere.SouthWest, res);
        }
    }
}
